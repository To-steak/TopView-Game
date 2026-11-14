"""코드 리뷰 원본 JSON의 지적 하나로 이슈 본문을 만든다.

본문은 이슈 폼 .github/ISSUE_TEMPLATE/review-finding.yml을 웹에서 채웠을 때와 같은 모양이다.
기본은 제목과 본문을 출력만 한다. --create를 주면 gh로 이슈를 열고, --edit 번호를 주면 그 이슈의 본문을 바꾼다.
실패한 테스트와 그 메시지는 Tools/Testing/run_tests.sh가 남긴 결과 파일(--results)에서 읽는다.
결과 파일에서 그 테스트가 실패하지 않았으면 이슈를 만들지 않는다.
출처와 관련 지적은 원본이 든 커밋의 그 지적 줄로 링크한다. 원본이 커밋·푸시되지 않았으면 이슈를 만들지 않는다.

예:
    python Tools/ReviewIssues/review_issue.py \
        --review Docs/Reviews/2026-11-14-game-code-review.json --finding 15 --commit 45ba8812 \
        --test MeleeOwnerDeathSystemTests.RequestsDespawnWhenEnemyOwnerIsDead \
        --results Project/Temp/TestResults/editmode.json --create
"""

import argparse
import json
import re
import subprocess
import sys
from urllib.parse import quote

REPOSITORY = 'To-steak/TopView-Game'
LABEL = 'bug'
NO_RESPONSE = '_No response_'
# 지적 객체만 가진 칸. 원본에서 지적이 시작하는 줄을 찾을 때 쓴다
FINDING_FIRST_FIELD = re.compile(r'\s*"file": ')


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


def run_git(arguments):
    result = subprocess.run(['git'] + arguments, text=True, encoding='utf-8', capture_output=True)
    if result.returncode != 0:
        sys.exit('git %s 실패: %s' % (' '.join(arguments), result.stderr.strip()))
    return result.stdout.strip()


def find_source_commit(review_path):
    """원본이 든 마지막 커밋을 돌려준다. 원본이 커밋되지 않았거나, 커밋 뒤에 바뀌었거나, 그 커밋이 푸시되지 않았으면 멈춘다.
    출처 링크가 GitHub에서 원본의 그 지적을 열어야 하기 때문이다."""
    if run_git(['status', '--porcelain', '--', review_path]):
        sys.exit('원본 %s가 커밋되지 않았거나 커밋 뒤에 바뀌었다. 원본만 커밋하고 푸시한 뒤 이슈를 등록한다.' % review_path)
    commit = run_git(['log', '-1', '--format=%h', '--abbrev=8', '--', review_path])
    if not commit:
        sys.exit('원본 %s가 든 커밋이 없다. 원본만 커밋하고 푸시한 뒤 이슈를 등록한다.' % review_path)
    if not run_git(['branch', '--remotes', '--contains', commit]):
        sys.exit('원본이 든 커밋 %s가 푸시되지 않아 출처 링크가 열리지 않는다. 푸시한 뒤 이슈를 등록한다.' % commit)
    return commit


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


def load_results(results_paths):
    """run_tests.sh의 결과 파일들을 읽어 테스트 결과 목록으로 돌려준다."""
    results = []
    for results_path in results_paths:
        with open(results_path, encoding='utf-8') as results_file:
            results.extend(json.load(results_file))
    return results


def find_failure(results, test):
    """결과 목록에서 테스트를 찾아 (전체 이름과 모드, 실패 메시지)를 돌려준다. 이름은 끝부분만 줘도 된다.
    찾지 못하거나 실패하지 않았으면 멈춘다. 재현하지 못한 지적이 이슈가 되지 않게 하기 위해서다."""
    matches = [item for item in results if item['fullName'] == test or item['fullName'].endswith('.' + test)]
    if len(matches) != 1:
        sys.exit('결과 파일에서 테스트 %s를 %d개 찾았다. 하나여야 한다.' % (test, len(matches)))
    match = matches[0]
    if match['status'] != 'Failed':
        sys.exit('테스트 %s의 결과가 %s라 이슈를 만들지 않는다. 실패한 테스트만 이슈의 증거가 된다.' % (test, match['status']))
    message = ' '.join(part.strip() for part in match['message'].splitlines() if part.strip())
    return '%s (%s)' % (match['fullName'], match['mode']), message


def build_failing_test(tests, failures):
    lines = []
    for index, test in enumerate(tests):
        lines.append('- `%s`' % test)
        if index < len(failures) and failures[index]:
            lines.append('  - %s' % failures[index])
    return '\n'.join(lines)


def build_body(review_path, number, finding, commit, source_commit, ranges, tests, failures, related):
    # 이슈 폼은 칸마다 "### 칸 이름" 아래에 입력값을 둔다
    sections = [
        ('출처', '`%s` %s' % (review_path, build_finding_link(review_path, number, source_commit, ranges))),
        ('위치', build_location(finding, commit)),
        ('분류', finding['category']),
        ('심각도', finding['severity']),
        ('리뷰어 확신', finding['confidence']),
        ('문제', finding['description']),
        ('재현 상황', finding['failureScenario']),
        ('근거', finding['evidence']),
        ('실패한 테스트', build_failing_test(tests, failures)),
        ('관련 지적', ', '.join(build_finding_link(review_path, value, source_commit, ranges) for value in related) if related else NO_RESPONSE),
    ]
    return '\n\n'.join('### %s\n\n%s' % (label, value) for label, value in sections) + '\n'


def main():
    # Windows 콘솔의 기본 인코딩으로는 한국어 메시지가 깨진다
    sys.stdout.reconfigure(encoding='utf-8')
    sys.stderr.reconfigure(encoding='utf-8')
    parser = argparse.ArgumentParser(description='코드 리뷰 원본 JSON의 지적 하나로 이슈 본문을 만든다.')
    parser.add_argument('--review', required=True, help='리뷰 원본 JSON 경로 (Docs/Reviews/*.json)')
    parser.add_argument('--finding', required=True, type=int, help='지적 번호. 원본에 나오는 순서로 1부터 센다')
    parser.add_argument('--commit', help='리뷰한 커밋. 위치 링크가 이 커밋의 줄을 가리킨다. 원본 JSON에 커밋이 있으면 주지 않는다')
    parser.add_argument('--test', required=True, action='append', help='고치기 전에 실패한 테스트 이름. 여러 번 줄 수 있다')
    parser.add_argument('--results', action='append', default=[], help='run_tests.sh가 남긴 결과 파일. 여러 번 줄 수 있다. 주면 --test의 실패 메시지를 여기서 읽는다')
    parser.add_argument('--failure', action='append', default=[], help='--results 없이 쓸 때 --test와 같은 순서의 실패 메시지')
    parser.add_argument('--related', action='append', type=int, default=[], help='같은 뿌리의 다른 지적 번호')
    parser.add_argument('--create', action='store_true', help='출력하지 않고 gh로 이슈를 연다')
    parser.add_argument('--edit', type=int, help='출력하지 않고 이 번호의 이슈 본문을 바꾼다. 제목은 그대로 둔다')
    arguments = parser.parse_args()

    if arguments.create and arguments.edit:
        sys.exit('--create와 --edit는 함께 줄 수 없다.')
    # 링크가 저장소 기준 경로를 쓰므로 Windows 구분자를 바꾼다
    review_path = arguments.review.replace('\\', '/')
    review_commit, findings = load_review(review_path)
    commit = arguments.commit or review_commit
    if not commit:
        sys.exit('원본 JSON에 리뷰한 커밋이 없어 --commit으로 줘야 한다.')
    if not 1 <= arguments.finding <= len(findings):
        sys.exit('지적 번호 %d가 범위(1~%d) 밖이다.' % (arguments.finding, len(findings)))
    for related in arguments.related:
        if not 1 <= related <= len(findings):
            sys.exit('관련 지적 번호 %d가 범위(1~%d) 밖이다.' % (related, len(findings)))

    tests = arguments.test
    failures = arguments.failure
    if arguments.results:
        results = load_results(arguments.results)
        pairs = [find_failure(results, test) for test in arguments.test]
        tests = [pair[0] for pair in pairs]
        failures = [pair[1] for pair in pairs]

    source_commit = find_source_commit(review_path)
    ranges = find_finding_ranges(review_path, len(findings))
    finding = findings[arguments.finding - 1]
    title = finding['title']
    body = build_body(review_path, arguments.finding, finding, commit, source_commit, ranges, tests, failures, arguments.related)

    if arguments.edit:
        command = ['gh', 'issue', 'edit', str(arguments.edit), '--repo', REPOSITORY, '--body-file', '-']
    elif arguments.create:
        command = ['gh', 'issue', 'create', '--repo', REPOSITORY, '--title', title, '--label', LABEL, '--body-file', '-']
    else:
        print('제목: %s\n' % title)
        print(body)
        return

    result = subprocess.run(command, input=body, text=True, encoding='utf-8', capture_output=True)
    if result.returncode != 0:
        sys.exit(result.stderr)
    print(result.stdout.strip())


if __name__ == '__main__':
    main()
