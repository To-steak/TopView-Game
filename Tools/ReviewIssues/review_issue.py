"""코드 리뷰 원본 JSON의 지적 하나와 그 판정으로 이슈 본문을 만든다.

본문은 이슈 폼 .github/ISSUE_TEMPLATE/review-finding.yml을 웹에서 채웠을 때와 같은 모양이다.
모든 지적이 이슈가 되고, 판정(--verdict)을 라벨로도 붙인다. reproduced만 열어 두고 나머지는 판정과 함께 닫는다.
기본은 제목과 본문을 출력만 한다. --create를 주면 gh로 이슈를 열고, --edit 번호를 주면 그 이슈의 본문을 바꾼다.

판정별로 필요한 것:
    reproduced          --test-commit, --test와 --results(또는 --failure). 결과 파일에서 그 테스트가 실패했어야 한다
    not-reproduced      --test-commit, --test, --results(또는 --failure), --reason. 결과 파일에서 그 테스트가 통과했어야 한다.
                        테스트 코드는 판정 커밋에서 읽어 본문에 접어 붙인다
    intended            --reason
    not-checked         --reason
    fixed-without-test  --reason (고친 커밋을 적는다)
출처와 관련 지적은 원본이 든 커밋의 그 지적 줄로, 확인한 테스트는 판정 커밋(--test-commit)의 테스트 메서드 줄로 링크한다.
원본이나 판정 커밋이 커밋·푸시되지 않았거나, 판정 커밋에서 테스트 메서드를 하나로 찾지 못하거나,
같은 지적을 출처나 관련 지적으로 가진 이슈가 이미 있으면 이슈를 만들지 않는다.

예:
    python Tools/ReviewIssues/review_issue.py \
        --review Docs/Reviews/2026-11-14-game-code-review.json --finding 15 --commit 45ba8812 \
        --verdict reproduced --test MeleeOwnerDeathSystemTests.RequestsDespawnWhenEnemyOwnerIsDead \
        --results Project/Temp/TestResults/editmode.json --create
"""

import argparse
import json
import re
import subprocess
import sys
from urllib.parse import quote

REPOSITORY = 'To-steak/TopView-Game'
FINDING_LABEL = 'review-finding'
NO_RESPONSE = '_No response_'
# 판정 커밋에서 테스트 클래스를 찾을 폴더. 테스트 폴더를 옮긴 적이 있어 스크립트 루트 전체에서 찾는다
TEST_ROOT = 'Project/Assets/Scripts'
# 판정마다 결과 파일에서 테스트가 가져야 할 상태와 이슈를 닫는 이유. 닫는 이유가 None이면 열어 둔다
VERDICTS = {
    'reproduced': {'test_status': 'Failed', 'close_reason': None},
    'not-reproduced': {'test_status': 'Passed', 'close_reason': 'not planned'},
    'intended': {'test_status': None, 'close_reason': 'not planned'},
    'not-checked': {'test_status': None, 'close_reason': 'not planned'},
    'fixed-without-test': {'test_status': None, 'close_reason': 'completed'},
}
# 지적 객체만 가진 칸. 원본에서 지적이 시작하는 줄을 찾을 때 쓴다
FINDING_FIRST_FIELD = re.compile(r'\s*"file": ')
# 이슈 본문의 출처와 관련 지적에서 원본 경로와 지적 번호를 읽는다
SOURCE_PATTERN = re.compile(r'### 출처\s+`([^`]+)` \[?지적 (\d+)')
RELATED_PATTERN = re.compile(r'### 관련 지적\s+([^\n]*)')


def load_review(review_path):
    """원본 JSON에서 리뷰한 커밋과 지적 목록을 돌려준다. 지적은 원본에 나오는 순서이고, 지적 번호는 이 순서(1부터)다.
    Tools/CodeReview/review.workflow.js로 만든 결과에는 커밋이 있고, 그 전의 결과(2026-11-14)에는 없어 None을 돌려준다."""
    with open(review_path, encoding='utf-8') as review_file:
        review = json.load(review_file)
    result = review['result']
    findings = []
    for area in result['areas']:
        findings.extend(area['findings'])
    return result.get('commit'), findings


def build_location(finding, commit):
    path = finding['file']
    line = finding['line']
    return '[%s:%d](https://github.com/%s/blob/%s/%s#L%d)' % (path, line, REPOSITORY, commit, quote(path), line)


def run_command(command, input_text=None):
    result = subprocess.run(command, input=input_text, text=True, encoding='utf-8', capture_output=True)
    if result.returncode != 0:
        sys.exit('%s 실패: %s' % (' '.join(command[:3]), result.stderr.strip()))
    return result.stdout.strip()


def find_source_commit(review_path):
    """원본이 든 마지막 커밋을 돌려준다. 원본이 커밋되지 않았거나, 커밋 뒤에 바뀌었거나, 그 커밋이 푸시되지 않았으면 멈춘다.
    출처 링크가 GitHub에서 원본의 그 지적을 열어야 하기 때문이다."""
    if run_command(['git', 'status', '--porcelain', '--', review_path]):
        sys.exit('원본 %s가 커밋되지 않았거나 커밋 뒤에 바뀌었다. 원본만 커밋하고 푸시한 뒤 이슈를 등록한다.' % review_path)
    commit = run_command(['git', 'log', '-1', '--format=%h', '--abbrev=8', '--', review_path])
    if not commit:
        sys.exit('원본 %s가 든 커밋이 없다. 원본만 커밋하고 푸시한 뒤 이슈를 등록한다.' % review_path)
    check_pushed(commit, '원본이 든 커밋')
    return commit


def check_pushed(commit, what):
    """커밋이 원격 브랜치에 있는지 본다. 없으면 GitHub 링크가 열리지 않으므로 멈춘다."""
    if not run_command(['git', 'branch', '--remotes', '--contains', commit]):
        sys.exit('%s %s가 푸시되지 않아 링크가 열리지 않는다. 푸시한 뒤 이슈를 등록한다.' % (what, commit))


def locate_test(test_commit, test):
    """판정 커밋에서 테스트 메서드가 있는 파일과 줄(1부터)을 찾아 (경로, 줄)로 돌려준다.
    테스트 이름은 '네임스페이스.클래스.메서드 (모드)' 꼴이다. 파일이나 메서드를 하나로 찾지 못하면 멈춘다."""
    name = test.split(' (')[0]
    parts = name.split('.')
    if len(parts) < 2:
        sys.exit('테스트 이름 %s에서 클래스와 메서드를 읽지 못했다. 결과 파일의 fullName을 준다.' % test)
    class_name, method_name = parts[-2], parts[-1].split('(')[0]
    # git grep의 확장 정규식은 \s와 \b를 모르므로 POSIX 문자 클래스로 쓴다
    grep = subprocess.run(['git', 'grep', '-l', '-E', 'class[[:space:]]+%s([^[:alnum:]_]|$)' % class_name, test_commit, '--', TEST_ROOT],
                          text=True, encoding='utf-8', capture_output=True)
    paths = [line.split(':', 1)[1] for line in grep.stdout.splitlines() if line]
    if len(paths) != 1:
        sys.exit('판정 커밋 %s에서 테스트 클래스 %s의 파일을 %d개 찾았다. 하나여야 한다.' % (test_commit, class_name, len(paths)))
    lines = run_command(['git', 'show', '%s:%s' % (test_commit, paths[0])]).splitlines()
    method_pattern = re.compile(r'\bpublic\b.*\b%s\s*\(' % re.escape(method_name))
    matches = [index + 1 for index, line in enumerate(lines) if method_pattern.search(line)]
    if len(matches) != 1:
        sys.exit('판정 커밋 %s의 %s에서 테스트 메서드 %s를 %d개 찾았다. 하나여야 한다.' % (test_commit, paths[0], method_name, len(matches)))
    return paths[0], matches[0]


def find_finding_ranges(review_path, count):
    """원본 파일에서 지적마다 그 객체가 차지하는 줄 범위(1부터, 처음과 끝)를 지적 순서대로 돌려준다.
    "file" 줄의 바로 위 { 줄부터, 그 줄과 들여쓰기가 같은 } 줄까지를 한 지적으로 본다."""
    with open(review_path, encoding='utf-8') as review_file:
        lines = review_file.read().splitlines()
    ranges = []
    for index, line in enumerate(lines):
        if not FINDING_FIRST_FIELD.match(line):
            continue
        start = index - 1
        indent = len(lines[start]) - len(lines[start].lstrip())
        end = start + 1
        while end < len(lines) and not (lines[end].lstrip().startswith('}') and len(lines[end]) - len(lines[end].lstrip()) == indent):
            end += 1
        ranges.append((start + 1, end + 1))
    if len(ranges) != count:
        sys.exit('원본에서 지적 줄을 %d개 찾았는데 지적은 %d개다. 원본이 줄바꿈된 JSON인지 확인한다.' % (len(ranges), count))
    return ranges


def build_finding_link(review_path, number, source_commit, ranges):
    start, end = ranges[number - 1]
    return '[지적 %d](https://github.com/%s/blob/%s/%s#L%d-L%d)' % (number, REPOSITORY, source_commit, quote(review_path), start, end)


def find_existing_issues(review_path, numbers):
    """출처나 관련 지적으로 이 원본의 지적 번호들을 가진 이슈를 (이슈 번호, 지적 번호) 목록으로 돌려준다."""
    issues = json.loads(run_command(['gh', 'issue', 'list', '--repo', REPOSITORY, '--state', 'all', '--limit', '1000', '--json', 'number,body']))
    matches = []
    for issue in issues:
        body = issue['body'] or ''
        found = []
        source = SOURCE_PATTERN.search(body)
        if source and source.group(1) == review_path:
            found.append(int(source.group(2)))
            related = RELATED_PATTERN.search(body)
            if related:
                found.extend(int(value) for value in re.findall(r'지적 (\d+)', related.group(1)))
        for number in found:
            if number in numbers:
                matches.append((issue['number'], number))
    return matches


def load_results(results_paths):
    """run_tests.sh의 결과 파일들을 읽어 테스트 결과 목록으로 돌려준다."""
    results = []
    for results_path in results_paths:
        with open(results_path, encoding='utf-8') as results_file:
            results.extend(json.load(results_file))
    return results


def find_result(results, test, expected_status):
    """결과 목록에서 테스트를 찾아 (전체 이름과 모드, 메시지)를 돌려준다. 이름은 끝부분만 줘도 된다.
    찾지 못하거나 판정이 요구하는 상태(reproduced는 실패, not-reproduced는 통과)가 아니면 멈춘다."""
    matches = [item for item in results if item['fullName'] == test or item['fullName'].endswith('.' + test)]
    if len(matches) != 1:
        sys.exit('결과 파일에서 테스트 %s를 %d개 찾았다. 하나여야 한다.' % (test, len(matches)))
    match = matches[0]
    if match['status'] != expected_status:
        sys.exit('테스트 %s의 결과가 %s인데 이 판정에는 %s여야 한다.' % (test, match['status'], expected_status))
    message = ' '.join(part.strip() for part in match['message'].splitlines() if part.strip())
    return '%s (%s)' % (match['fullName'], match['mode']), message or '통과'


def build_checked_test(tests, messages, test_commit, is_code_attached):
    """확인한 테스트마다 판정 커밋의 메서드 줄 링크와 결과 메시지를 적는다. is_code_attached면 그 커밋의 테스트 파일을 접어 붙인다."""
    if not tests:
        return NO_RESPONSE
    lines = []
    attached_paths = []
    for index, test in enumerate(tests):
        path, line = locate_test(test_commit, test)
        lines.append('- [`%s`](https://github.com/%s/blob/%s/%s#L%d)' % (test, REPOSITORY, test_commit, quote(path), line))
        if index < len(messages) and messages[index]:
            lines.append('  - %s' % messages[index])
        if is_code_attached and path not in attached_paths:
            attached_paths.append(path)
    for path in attached_paths:
        code = run_command(['git', 'show', '%s:%s' % (test_commit, path)]).rstrip()
        lines.append('\n<details><summary>%s (판정 커밋 %s의 코드, 다음 커밋에서 지움)</summary>\n\n```csharp\n%s\n```\n\n</details>' % (path.split('/')[-1], test_commit, code))
    return '\n'.join(lines)


def build_body(review_path, number, finding, commit, source_commit, ranges, verdict, reason, checked_test, related):
    # 이슈 폼은 칸마다 "### 칸 이름" 아래에 입력값을 둔다
    sections = [
        ('출처', '`%s` %s' % (review_path, build_finding_link(review_path, number, source_commit, ranges))),
        ('위치', build_location(finding, commit)),
        ('분류', finding['category']),
        ('심각도', finding['severity']),
        ('리뷰어 확신', finding['confidence']),
        ('판정', verdict),
        ('판정 이유', reason or NO_RESPONSE),
        ('문제', finding['description']),
        ('재현 상황', finding['failureScenario']),
        ('근거', finding['evidence']),
        ('확인한 테스트', checked_test),
        ('관련 지적', ', '.join(build_finding_link(review_path, value, source_commit, ranges) for value in related) if related else NO_RESPONSE),
    ]
    return '\n\n'.join('### %s\n\n%s' % (label, value) for label, value in sections) + '\n'


def check_arguments(arguments, findings):
    if arguments.create and arguments.edit:
        sys.exit('--create와 --edit는 함께 줄 수 없다.')
    for number in [arguments.finding] + arguments.related:
        if not 1 <= number <= len(findings):
            sys.exit('지적 번호 %d가 범위(1~%d) 밖이다.' % (number, len(findings)))
    if VERDICTS[arguments.verdict]['test_status']:
        if not arguments.test or not arguments.test_commit:
            sys.exit('%s 판정에는 --test와 --test-commit(판정 커밋)이 있어야 한다.' % arguments.verdict)
        if not arguments.results and len(arguments.failure) != len(arguments.test):
            sys.exit('--results가 없으면 --test마다 --failure로 결과 메시지를 줘야 한다.')
    elif arguments.test or arguments.test_commit:
        sys.exit('%s 판정에는 테스트를 붙이지 않는다.' % arguments.verdict)
    if arguments.verdict != 'reproduced' and not arguments.reason:
        sys.exit('%s 판정에는 --reason이 있어야 한다.' % arguments.verdict)


def main():
    # Windows 콘솔의 기본 인코딩으로는 한국어 메시지가 깨진다
    sys.stdout.reconfigure(encoding='utf-8')
    sys.stderr.reconfigure(encoding='utf-8')
    parser = argparse.ArgumentParser(description='코드 리뷰 원본 JSON의 지적 하나와 그 판정으로 이슈 본문을 만든다.')
    parser.add_argument('--review', required=True, help='리뷰 원본 JSON 경로 (Docs/Reviews/*.json)')
    parser.add_argument('--finding', required=True, type=int, help='지적 번호. 원본에 나오는 순서로 1부터 센다')
    parser.add_argument('--verdict', required=True, choices=list(VERDICTS), help='판정. 라벨로도 붙는다')
    parser.add_argument('--reason', help='판정 이유. reproduced가 아니면 있어야 한다')
    parser.add_argument('--commit', help='리뷰한 커밋. 위치 링크가 이 커밋의 줄을 가리킨다. 원본 JSON에 커밋이 있으면 주지 않는다')
    parser.add_argument('--test', action='append', default=[], help='확인한 테스트 이름. 여러 번 줄 수 있다')
    parser.add_argument('--results', action='append', default=[], help='run_tests.sh가 남긴 결과 파일. 여러 번 줄 수 있다. 주면 --test의 결과를 여기서 읽는다')
    parser.add_argument('--failure', action='append', default=[], help='--results 없이 쓸 때 --test와 같은 순서의 결과 메시지')
    parser.add_argument('--test-commit', help='리뷰 원본과 테스트를 올린 판정 커밋. 확인한 테스트를 이 커밋의 메서드 줄로 링크한다')
    parser.add_argument('--related', action='append', type=int, default=[], help='같은 뿌리라 이 이슈로 함께 다루는 다른 지적 번호')
    parser.add_argument('--create', action='store_true', help='출력하지 않고 gh로 이슈를 열고, 판정대로 라벨을 붙이고 닫는다')
    parser.add_argument('--edit', type=int, help='출력하지 않고 이 번호의 이슈 본문을 바꾼다. 제목, 라벨, 열림 상태는 그대로 둔다')
    arguments = parser.parse_args()

    # 링크가 저장소 기준 경로를 쓰므로 Windows 구분자를 바꾼다
    review_path = arguments.review.replace('\\', '/')
    review_commit, findings = load_review(review_path)
    commit = arguments.commit or review_commit
    if not commit:
        sys.exit('원본 JSON에 리뷰한 커밋이 없어 --commit으로 줘야 한다.')
    check_arguments(arguments, findings)

    tests = arguments.test
    messages = arguments.failure
    expected_status = VERDICTS[arguments.verdict]['test_status']
    if expected_status and arguments.results:
        results = load_results(arguments.results)
        pairs = [find_result(results, test, expected_status) for test in arguments.test]
        tests = [pair[0] for pair in pairs]
        messages = [pair[1] for pair in pairs]

    source_commit = find_source_commit(review_path)
    ranges = find_finding_ranges(review_path, len(findings))
    finding = findings[arguments.finding - 1]
    title = finding['title']
    test_commit = None
    if arguments.test_commit:
        test_commit = run_command(['git', 'rev-parse', '--short=8', arguments.test_commit + '^{commit}'])
        check_pushed(test_commit, '판정 커밋')
    checked_test = build_checked_test(tests, messages, test_commit, arguments.verdict == 'not-reproduced')
    body = build_body(review_path, arguments.finding, finding, commit, source_commit, ranges, arguments.verdict, arguments.reason, checked_test, arguments.related)

    if arguments.edit:
        print(run_command(['gh', 'issue', 'edit', str(arguments.edit), '--repo', REPOSITORY, '--body-file', '-'], body))
        return
    if not arguments.create:
        print('제목: %s\n' % title)
        print(body)
        return

    existing = find_existing_issues(review_path, [arguments.finding] + arguments.related)
    if existing:
        sys.exit('이미 이 지적을 다루는 이슈가 있다: %s' % ', '.join('#%d(지적 %d)' % pair for pair in existing))
    url = run_command(['gh', 'issue', 'create', '--repo', REPOSITORY, '--title', title, '--label', '%s,%s' % (FINDING_LABEL, arguments.verdict), '--body-file', '-'], body)
    close_reason = VERDICTS[arguments.verdict]['close_reason']
    if close_reason:
        run_command(['gh', 'issue', 'close', url, '--repo', REPOSITORY, '--reason', close_reason])
    print('%s (%s%s)' % (url, arguments.verdict, ', 닫음: ' + close_reason if close_reason else ''))


if __name__ == '__main__':
    main()
