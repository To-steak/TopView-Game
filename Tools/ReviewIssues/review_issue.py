"""코드 리뷰 원본 JSON의 지적 하나로 이슈 본문을 만든다.

본문은 이슈 폼 .github/ISSUE_TEMPLATE/review-finding.yml을 웹에서 채웠을 때와 같은 모양이다.
기본은 제목과 본문을 출력만 한다. --create를 주면 gh로 이슈를 연다.

예:
    python Tools/ReviewIssues/review_issue.py \
        --review Docs/Reviews/2026-11-14-game-code-review.json --finding 15 --commit 45ba8812 \
        --test "MeleeOwnerDeathSystemTests.RequestsDespawnWhenEnemyOwnerIsDead" \
        --failure "Expected: 1  But was: 0"
"""

import argparse
import json
import subprocess
import sys

REPOSITORY = 'To-steak/TopView-Game'
LABEL = 'bug'
NO_RESPONSE = '_No response_'


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
    return '[%s:%d](https://github.com/%s/blob/%s/%s#L%d)' % (path, line, REPOSITORY, commit, path, line)


def build_failing_test(tests, failures):
    lines = []
    for index, test in enumerate(tests):
        lines.append('- `%s`' % test)
        if index < len(failures) and failures[index]:
            lines.append('  - %s' % failures[index])
    return '\n'.join(lines)


def build_body(review_path, number, finding, commit, tests, failures, related):
    # 이슈 폼은 칸마다 "### 칸 이름" 아래에 입력값을 둔다
    sections = [
        ('출처', '`%s` 지적 %d' % (review_path, number)),
        ('위치', build_location(finding, commit)),
        ('분류', finding['category']),
        ('심각도', finding['severity']),
        ('리뷰어 확신', finding['confidence']),
        ('문제', finding['description']),
        ('재현 상황', finding['failureScenario']),
        ('근거', finding['evidence']),
        ('실패한 테스트', build_failing_test(tests, failures)),
        ('관련 지적', ', '.join('지적 %d' % value for value in related) if related else NO_RESPONSE),
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
    parser.add_argument('--failure', action='append', default=[], help='--test와 같은 순서의 실패 메시지')
    parser.add_argument('--related', action='append', type=int, default=[], help='같은 뿌리의 다른 지적 번호')
    parser.add_argument('--create', action='store_true', help='출력하지 않고 gh로 이슈를 연다')
    arguments = parser.parse_args()

    review_commit, findings = load_review(arguments.review)
    commit = arguments.commit or review_commit
    if not commit:
        sys.exit('원본 JSON에 리뷰한 커밋이 없어 --commit으로 줘야 한다.')
    if not 1 <= arguments.finding <= len(findings):
        sys.exit('지적 번호 %d가 범위(1~%d) 밖이다.' % (arguments.finding, len(findings)))
    for related in arguments.related:
        if not 1 <= related <= len(findings):
            sys.exit('관련 지적 번호 %d가 범위(1~%d) 밖이다.' % (related, len(findings)))

    finding = findings[arguments.finding - 1]
    title = finding['title']
    body = build_body(arguments.review, arguments.finding, finding, commit, arguments.test, arguments.failure, arguments.related)

    if not arguments.create:
        print('제목: %s\n' % title)
        print(body)
        return

    result = subprocess.run(
        ['gh', 'issue', 'create', '--repo', REPOSITORY, '--title', title, '--label', LABEL, '--body-file', '-'],
        input=body, text=True, encoding='utf-8', capture_output=True)
    if result.returncode != 0:
        sys.exit(result.stderr)
    print(result.stdout.strip())


if __name__ == '__main__':
    main()
