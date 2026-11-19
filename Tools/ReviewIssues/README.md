# 리뷰 이슈 도구

`review_issue.py`는 리뷰 원본 JSON(`Docs/Reviews/`)의 지적 하나와 그 판정으로 GitHub 이슈를 만든다. 모든 지적이 이슈가 되고, 판정을 라벨로 붙인다. 테스트로 재현된 `reproduced`만 열어 두고, 나머지는 판정과 함께 닫는다.

리뷰부터 이슈 등록까지의 전체 흐름은 [코드 리뷰 도구 README](../CodeReview/README.md)에, 실제 진행 순서는 스킬 `/review-to-issues`(`.claude/skills/review-to-issues/SKILL.md`)에 있다. 이 도구는 그중 이슈 등록을 맡는다.

```sh
python Tools/ReviewIssues/review_issue.py \
    --review Docs/Reviews/2026-11-14-game-code-review.json --finding 15 --commit 45ba8812 \
    --verdict reproduced --test-commit <판정 커밋> \
    --test MeleeOwnerDeathSystemTests.RequestsDespawnWhenEnemyOwnerIsDead \
    --results Project/Temp/TestResults/editmode.json --create
```

- 기본은 이슈 제목과 본문을 출력만 한다
- `--create`: `gh`로 이슈를 열고, `review-finding`과 판정 라벨을 붙이고, 판정에 따라 닫는다
- `--edit 번호`: 그 이슈의 본문만 바꾼다. 제목, 라벨, 열림 상태는 그대로 둔다

## 필요한 것

- Python 3: 표준 라이브러리만 쓴다
- GitHub CLI(`gh`): 저장소에 로그인해 있어야 한다
- Git: 원본과 판정 커밋이 푸시됐는지 확인하고, 판정 커밋에서 테스트 코드를 읽는다
- 테스트 결과 파일: `Tools/Testing/run_tests.sh`가 남긴 `Project/Temp/TestResults/<mode>.json` ([테스트 실행 도구](../Testing/README.md))

## 판정

| 판정 | 뜻 | 필요한 인자 | 이슈 |
|---|---|---|---|
| `reproduced` | 테스트로 재현됨 | `--test-commit`, `--test`, `--results`(또는 `--failure`) | 열어 둠 |
| `not-reproduced` | 테스트를 썼지만 재현되지 않음 | 위와 같고 `--reason` 추가 | 닫음 (not planned) |
| `intended` | 의도한 동작 | `--reason` | 닫음 (not planned) |
| `not-checked` | 테스트로 확인하지 않음(규칙, 정리 등) | `--reason` | 닫음 (not planned) |
| `fixed-without-test` | 테스트 없이 고침 | `--reason` (고친 커밋을 적는다) | 닫음 (completed) |

- `reproduced`는 결과 파일에서 그 테스트가 **실패**했어야 하고, `not-reproduced`는 **통과**했어야 한다. 아니면 이슈를 만들지 않는다.
  - 이유: 판정이 실제 테스트 결과와 어긋나지 않게 한다.
- `not-reproduced`는 판정 커밋의 테스트 코드를 본문에 접어 붙인다.
  - 이유: 재현되지 않은 테스트는 다음 커밋에서 지우므로, 무엇을 확인했는지 이슈에 남긴다.

## 이슈를 만들지 않는 경우

- 원본이 커밋되지 않았거나, 커밋 뒤에 바뀌었거나, 원본이 든 커밋이 푸시되지 않았다
- 판정 커밋이 푸시되지 않았다
- 판정 커밋에서 테스트 클래스나 메서드를 하나로 찾지 못했다
- 같은 지적을 출처나 관련 지적으로 가진 이슈가 이미 있다
- 이유: 이슈 본문의 링크(출처 지적, 리뷰한 코드 줄, 확인한 테스트 줄)가 GitHub에서 열려야 하고, 같은 지적이 두 이슈로 나뉘면 안 된다.

## 이슈 본문

이슈 폼 `.github/ISSUE_TEMPLATE/review-finding.yml`을 웹에서 채웠을 때와 같은 모양으로 만든다. 칸은 출처, 위치, 분류, 심각도, 리뷰어 확신, 판정, 판정 이유, 문제, 재현 상황, 근거, 확인한 테스트, 관련 지적이다.

- 출처와 관련 지적: 원본이 든 커밋의 그 지적 줄로 링크한다
- 위치: 리뷰한 커밋의 코드 줄로 링크한다
- 확인한 테스트: 판정 커밋의 테스트 메서드 줄로 링크한다
- `--related 번호`: 같은 뿌리라 한 이슈로 함께 다루는 지적. 그 지적들은 이슈를 따로 만들지 않는다

## 함께 바꿔야 하는 파일

- 지적의 JSON 스키마를 바꾸면: 리뷰 스크립트 둘(`Tools/CodeReview/*.workflow.js`), 이슈 폼, 이 도구를 함께 바꾼다([코드 리뷰 도구 README](../CodeReview/README.md#결과-형식을-바꿀-때))
- 판정이나 인자를 바꾸면: 스킬 `.claude/skills/review-to-issues/SKILL.md`의 명령 예시를 함께 바꾼다
- 이슈 본문의 칸을 바꾸면: 이슈 폼과 이 도구의 `build_body`를 함께 바꾼다. 이미 있는 이슈를 찾는 정규식(`SOURCE_PATTERN`, `RELATED_PATTERN`)이 출처와 관련 지적 칸을 읽는다
