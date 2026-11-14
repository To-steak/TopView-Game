---
name: review-to-issues
description: TopView 코드를 리뷰하고, 지적마다 테스트나 사용자 판단으로 판정을 정한 뒤, 리뷰 원본을 커밋·푸시하고 모든 지적을 판정과 함께 GitHub 이슈로 등록한다. 재현된 버그만 열어 둔다. 기능(feat)을 마친 뒤 그 범위를 리뷰할 때 쓴다. 고치기와 테스트 커밋은 하지 않는다.
---

# 리뷰 → 판정(테스트, 질문) → 원본 커밋 → 이슈 등록

코드 리뷰의 지적은 추측이다. 이 스킬은 지적마다 판정을 정해 **모든 지적을 이슈로 남기고**, 고치기 전 코드에서 실패하는 테스트가 있는 지적(`reproduced`)만 열어 둔다. 나머지는 판정과 함께 닫아 기록으로 남긴다. 이슈 등록에서 끝난다. 커밋은 리뷰 원본 하나만 하고 바로 푸시한다. 고치기와 테스트의 커밋은 하지 않는다.

도구:
- 리뷰: `Tools/CodeReview/review.workflow.js` (워크플로)
- 테스트 실행: `sh Tools/Testing/run_tests.sh editmode|playmode [필터]` → 결과 `Project/Temp/TestResults/<mode>.json`
- 이슈 등록: `python Tools/ReviewIssues/review_issue.py ... --results <결과 파일> --create`

이 스킬을 부른 것 자체가 워크플로 실행에 대한 동의다.

## 1. 리뷰 범위를 정하고 보여 준다

사용자가 말한 범위로 `args`를 만든다. 범위가 없으면 묻는다. 보통은 방금 끝낸 기능이 건드린 폴더다.

```json
{
  "date": "YYYY-MM-DD (오늘)",
  "commit": "git rev-parse --short HEAD의 값",
  "context": "이번 리뷰의 상황 (어떤 기능을 마쳤는지 등). 없으면 빈 문자열",
  "areas": [
    { "key": "A1", "name": "A1. 영역 이름", "scope": ["Project/Assets/Scripts/..."], "focus": "특히 볼 것" }
  ]
}
```

- 영역 하나는 끝까지 읽을 수 있는 양(대략 2,500~5,000줄)으로 나눈다. 에이전트 수는 영역 수와 같다.
- 리뷰할 커밋은 작업 트리가 깨끗한 상태여야 한다. 커밋하지 않은 변경이 있으면 리뷰 결과의 줄 번호가 커밋과 어긋나므로 사용자에게 알리고 멈춘다.
- `args`와 리뷰 결과 파일 이름(`Docs/Reviews/날짜-주제.json`)을 보여 주고 확인받은 뒤 실행한다. 워크플로는 비용이 커서 이 확인은 생략하지 않는다.

## 2. 리뷰를 실행하고 원본을 저장한다

- `Workflow({ scriptPath: "Tools/CodeReview/review.workflow.js", args })`로 실행한다. 백그라운드로 돌고, 끝나면 알림이 온다.
- 알림의 결과 파일(task output)을 그대로 `Docs/Reviews/날짜-주제.json`으로 복사한다. 원본은 고치지 않는다.
- 지적 번호는 원본의 `result.areas[].findings[]`를 순서대로 이어 센 번호(1부터)다.

## 3. 지적을 나눈다

판정은 다섯 가지다. 뜻은 이슈 폼(`.github/ISSUE_TEMPLATE/review-finding.yml`)의 판정 칸에 있다.

- 같은 뿌리의 지적은 묶는다. 대표 하나만 판정하고, 나머지는 그 이슈의 관련 지적(`--related`)으로 넣는다.
- `rule`, `cleanup`은 `not-checked`로 둔다. 테스트로 확인할 동작이 없다.
- `bug`, `inconsistency` 중 지적대로 동작해도 기획 의도일 수 있는 것(지적의 description이 의도일 수 있다고 적었거나, 수치·표시 방식의 선택인 것)은 테스트를 쓰기 전에 한 번에 묻는다. 의도라고 하면 `intended`로 두고 테스트하지 않는다. 사용자가 답한 내용을 판정 이유에 적는다.
- 나머지 `bug`, `inconsistency`는 4단계에서 테스트한다.

## 4. 지적마다 실패하는 테스트를 쓰고 돌린다

지적의 `failureScenario`를 재현하는 테스트를 쓴다. 규칙은 `.claude/rules/architecture.md`의 `Tests/`, `PlayModeTests/` 항목을 따른다.

- **System 하나로 드러나면 EditMode** (`Project/Assets/Scripts/Tests/`): 필요한 칸만 채운 `EntityStorage`와 큐를 만들어 `Tick`을 부른다. 클래스 이름은 `<System 이름>Tests`.
- **World의 호출 순서, 씬(지형, NavMesh)에 달리면 PlayMode** (`Project/Assets/Scripts/PlayModeTests/`): `WorldTestScene`으로 씬을 불러와 `World.Test.cs` 통로로 상황을 만들고 `world.Tick()`을 직접 부른다. 통로에 필요한 값이 없으면 `World.Test.cs`에 더한다.
- **표시(ViewSystem, UI View, Presenter)에 달리면 PlayMode에서 대상 하나 단위로**: 필요한 참조만 붙인 View를 테스트가 만들고 `Present`나 표시 메서드를 부른다. private 직렬화 필드는 `SerializedFieldWriter`로 채운다. 클래스 이름은 `<대상 이름>Tests`.
- 확인 줄에는 무엇이 틀렸는지 알 수 있는 메시지를 단다. 이 메시지가 이슈의 증거로 들어간다.
- 정상 동작을 지키는 대조 테스트를 함께 쓰면, 테스트가 버그만 골라 잡는지 보인다.

테스트를 돌린다. EditMode는 여러 지적을 모아 한 번에, PlayMode는 몇 분 걸리므로 모아서 백그라운드로 돌린다.

```sh
sh Tools/Testing/run_tests.sh editmode
sh Tools/Testing/run_tests.sh playmode
```

결과를 판정한다.

| 결과 | 판정 | 할 일 |
|---|---|---|
| 지적을 재현하는 확인 줄에서 실패 | `reproduced` | 테스트를 남긴다(커밋은 고칠 때) |
| 통과 | `not-reproduced` | 테스트 코드를 scratchpad에 복사해 둔 뒤 저장소에서 `.meta`까지 지운다. 복사본은 이슈 본문에 넣는다 |
| 씬 로드, 준비 단계, 컴파일에서 실패 | 테스트가 틀림 | 테스트를 고쳐 다시 돌린다. 재현으로 보지 않는다 |

## 5. 리뷰 원본을 커밋하고 푸시한다

이슈의 출처와 관련 지적은 원본이 든 커밋의 그 지적 줄로 링크된다. 그래서 이슈를 등록하기 전에 원본을 GitHub에 올린다. 묻지 않고 한다.

```sh
git add Docs/Reviews/날짜-주제.json
git commit -m "docs: <리뷰 범위> 리뷰 원본을 남김"
git push
```

- 원본 하나만 커밋한다. 남긴 실패 테스트는 넣지 않는다(고치는 커밋에 함께 넣는다).
- 재현된 지적이 없어도 원본은 커밋한다. 무엇을 리뷰했는지의 기록이다.
- 지금 브랜치에 푸시하지 않은 다른 커밋이 있으면 함께 올라간다. 보고할 때 알린다.

## 6. 모든 지적을 판정과 함께 이슈로 등록한다

지적마다(묶은 지적은 대표만) 바로 등록한다. 본문을 따로 보여 주거나 확인받지 않는다. 도구가 `review-finding`과 판정 라벨을 붙이고, `reproduced`가 아니면 닫는다.

```sh
# reproduced
python Tools/ReviewIssues/review_issue.py --review Docs/Reviews/날짜-주제.json --finding <번호> [--related <번호>] \
    --verdict reproduced --test <테스트 이름> [--test ...] \
    --results Project/Temp/TestResults/editmode.json [--results Project/Temp/TestResults/playmode.json] --create
# not-reproduced
python Tools/ReviewIssues/review_issue.py --review ... --finding <번호> --verdict not-reproduced \
    --test <테스트 이름> --results <결과 파일> --test-file <scratchpad에 복사한 테스트 코드> --reason "<무엇을 만들어 봤는지>" --create
# intended, not-checked
python Tools/ReviewIssues/review_issue.py --review ... --finding <번호> --verdict intended --reason "<사용자가 답한 내용>" --create
```

- 테스트 이름은 결과 파일의 `fullName` 끝부분만 줘도 된다.
- 결과 파일에서 그 테스트의 상태가 판정과 맞지 않으면(reproduced인데 통과, not-reproduced인데 실패) 도구가 등록을 거부한다. 이때는 4단계 판정으로 돌아간다.
- 같은 지적을 출처나 관련 지적으로 가진 이슈가 이미 있으면 도구가 등록을 거부한다.
- 원본이 커밋되지 않았거나, 고친 채이거나, 원본이 든 커밋이 푸시되지 않았으면 도구가 등록을 거부한다. 이때는 5단계로 돌아간다.
- 2026-11-14 리뷰처럼 원본에 커밋이 없으면 `--commit`을 준다.

## 7. 보고한다

- 리뷰 원본 경로와 지적 수, 원본을 커밋한 커밋(함께 푸시된 다른 커밋이 있으면 그것도)
- 판정별 이슈 수와, 열어 둔 이슈(`reproduced`)의 번호, 제목, 증거가 된 테스트
- `not-reproduced`로 닫은 이슈와 지운 테스트
- 남긴 테스트 파일은 커밋하지 않은 상태다. 고칠 때 수정과 함께 `Fixes #번호`로 커밋한다고 알린다.
