# CLAUDE.md

## 프로젝트 값
`.claude/rules/`의 규칙은 프로젝트에 묶이지 않은 원칙만 담는다. 이 프로젝트에서의 구체 값은 여기에 둔다.

- 엔진: Unity 6 (6000.3.27f2), URP. 타겟 플랫폼은 PC다.
- Unity 프로젝트 루트: `Project/`
- 스크립트 루트: `Project/Assets/Scripts/`
- 어셈블리·네임스페이스 접두사: `TopView`
- Fixed Timestep: 0.02초 (50Hz)
- 외부 패키지 참조
  - `TopView.Core` → `Unity.InputSystem`
  - `TopView.Presentation` → `Unity.Cinemachine`, `UnityEngine.UI`, `Unity.TextMeshPro`, `DOTween.Modules`
  - `TopView.UserInterface` → `UnityEngine.UI`, `Unity.TextMeshPro`, `DOTween.Modules`
  - `TopView.Editor` → `UnityEngine.UI`
  - `TopView.Tests.PlayMode` → `UnityEngine.UI`, `Unity.TextMeshPro`

## 대화
- 응답은 항상 한국어로 한다.
- 질문에는 답만 한다. 파일 수정은 작업을 요청받았을 때만 한다.
- 새 시스템이나 규칙을 도입해야 하면 코드 작성 전에 묻는다.
- 설계에 값이 없는 작은 수치나 동작은 정해서 진행하되, 응답에 무엇을 정했는지 알린다.
- 지시가 `.claude/rules/`나 이 문서의 규칙과 어긋나면, 작업을 시작하기 전에 어긋나는 규칙과 그 규칙의 이유를 말하고 되묻는다. 사용자가 그래도 진행하라고 하면 지시를 따르고, 규칙을 고칠지 이번만 예외로 둘지 함께 확인한다.
  - 이유: 규칙을 조용히 어기면 문서와 코드가 어긋나고, 지시를 조용히 무시하면 사용자의 결정이 사라진다.
- 알고 있는 부작용(기능이 줄거나 동작이 바뀌는 것)이 있으면 작업을 시작하기 전에 말한다.
  - 이유: 작업 후에 알리면 되돌리는 작업이 생긴다.
- 이전 결정을 문제로 꼽기 전에, 그 결정의 이유가 아직 유효한지 먼저 확인한다.
  - 이유: 이유 있는 결정을 실수처럼 말하면 고칠 필요 없는 것을 고치게 된다.

## 설계
- 설계는 사용자가 요청에 붙여 준다. `feat` 작업인데 설계가 없거나, 붙여 준 설계에 필요한 내용이 없으면 묻는다.

## 요청 범위
- 요청한 기능의 핵심 로직만 만든다. 연출(흔들림 등), HUD, 디버그 표시(OnGUI), 에디터 기즈모처럼 딸려 오는 것은 스스로 판단해 덧붙이지 않는다. 필요해 보이면 코드를 쓰기 전에 하나씩 묻는다.
  - 이유: 요청하지 않은 결정이 섞이면 검토할 양이 늘고, 원하지 않은 부분을 골라내 되돌리기 어렵다.
- 확인 질문을 묶어서 했는데 답이 짧거나 모호하면, 범위를 넓히는 쪽으로 해석하지 않고 다시 묻는다.
  - 이유: "네" 한 마디를 모든 항목에 대한 허락으로 보면 사용자가 의도하지 않은 범위까지 구현하게 된다.

## 작업 범위
Claude는 코드, 도구, 기록, 문서만 쓴다. 만들고 수정하는 파일은 다음뿐이다.

- Unity 프로젝트 안: 스크립트 루트 아래의 `.cs`, `.asmdef`, `.asmref`와, 정적 분석기를 빌드해 `Project/Assets/Analyzers/`에 복사하는 `TopView.Analyzers.dll` (`.meta`는 사용자가 다룬다)
- 정적 분석기: 소스 `Tools/TopView.Analyzers/`와 테스트 `Tools/TopView.Analyzers.Tests/`
  - 이유: 분석기는 Unity가 아닌 .NET 라이브러리로 빌드해야 해서, 소스를 `Assets` 안에 두면 Unity가 게임 코드로 컴파일하려 한다.
- 리뷰와 테스트 도구: `Tools/CodeReview/`, `Tools/ReviewIssues/`, `Tools/Testing/`, 이슈 폼 `.github/ISSUE_TEMPLATE/`, 프로젝트 스킬 `.claude/skills/`
  - 도구를 고치기 전에 그 도구 폴더의 `README.md`를 읽는다. 함께 바꿔야 하는 파일이 거기 있다.
- 기록: 리뷰 결과, 도구 측정처럼 이슈와 커밋이 근거로 가리키는 기록은 `Docs/` 아래에 둔다.
  - 이유: 이슈, 커밋, 테스트가 무엇을 근거로 했는지 저장소에서 따라갈 수 있어야 한다. 대화나 임시 폴더에만 있으면 사라진다.
- 문서: 저장소의 `.md` 문서(루트 `README.md`, 이 문서 `CLAUDE.md`, 규칙 `.claude/rules/`)는 사용자와 함께 쓰고, 요청받은 부분만 고친다. 문장에는 저장소에서 확인한 사실만 쓴다.
  - 이유: README는 사용자가 밖에 내보이는 문서이고, `CLAUDE.md`와 규칙은 Claude가 따르는 기준이다. 확인하지 않은 내용이 섞이면 사용자가 설명하지 못하는 주장이 되고, 요청 없이 고치면 사용자가 정한 기준이 바뀐다.

나머지(씬, 프리팹, 에셋, 설정, `.meta`)는 사용자가 에디터에서 다룬다.

## 에디터와 파일
- Claude는 에디터를 직접 조작하지 않는다. 에디터에서 할 일(씬, 프리팹, 에셋, 설정)이 생기면 응답에 구체적으로 적는다.
- 코드를 고친 뒤에는 `unity recompile --project-path Project`로 실행 중인 에디터의 컴파일 오류를 확인하고, 오류가 있으면 고친다. 연결된 에디터가 없으면(`unity status`) 확인하지 못했다고 알리고, 에디터를 띄우거나 Play 모드에 들어가지 않는다.
  - 이유: 컴파일 오류를 Claude가 먼저 잡으면 사용자가 확인하는 일이 동작 검토로 줄어든다. 에디터 상태(열기, Play 모드)는 사용자의 작업을 끊을 수 있어 사용자가 정한다.
- **파일을 옮길 때는 짝인 `.meta`와 함께 `git mv`로 옮긴다.** 새 폴더의 `.meta`는 만들지 않고 Unity가 만들게 둔다. 파일 이름은 바꾸지 않는다. 클래스 이름을 바꿔야 하면 사용자가 에디터에서 파일 이름을 먼저 바꾼 뒤 코드를 고친다.
  - 이유: GUID는 `.meta`에 있어서, `.cs`만 옮기거나 `.meta`를 새로 만들면 GUID가 끊어져 에셋과 씬의 참조가 사라진다. 짝을 함께 옮기면 GUID가 그대로 따라간다.

## 정적 분석기
- 빌드: `dotnet build Tools/TopView.Analyzers -c Release` (빌드가 DLL을 `Project/Assets/Analyzers/`로 복사한다)
- 분석기를 고치면 `dotnet test Tools/TopView.Analyzers.Tests`로 확인한다. 규칙을 더하거나 바꾸면 먼저 그 규칙의 예시 문서(`Tools/TopView.Analyzers.Tests/Examples/`)를 고치고, 분석기를 예시에 맞춘다.
  - 이유: 사용자는 분석기 코드(Roslyn)가 아니라 예시 문서로 규칙의 동작을 확인한다. 예시가 곧 테스트라 문서와 분석기가 어긋나지 않는다.

## 테스트
- `sh Tools/Testing/run_tests.sh editmode|playmode [필터]`로 돌린다. 결과는 `Project/Temp/TestResults/<mode>.json`에 남는다. 자세한 동작은 `Tools/Testing/README.md`에 있다.
- editmode는 사용자가 연 에디터에서 돌린다. 연결된 에디터가 없으면 돌리지 않는다.
- playmode는 테스트 전용 워크 트리 `../TopView-Game-Test`에서 배치 모드 에디터로 돌린다. 사용자가 연 에디터는 건드리지 않는다. 오래 걸리므로 백그라운드로 돌린다.
  - 이유: 다른 폴더의 배치 모드 에디터는 사용자의 에디터와 씬을 끊지 않는다. 같은 프로젝트 폴더는 에디터 둘이 동시에 열 수 없다.
- 한 틱 차이로 생기는 버그는 사람이 에디터에서 재현하기 어려우므로, 주로 테스트로 확인한다.

## 코드 리뷰
리뷰는 스킬 `/review-to-issues`로 한다. 리뷰 → 판정(테스트) → 판정 커밋 → 이슈 등록의 절차는 스킬(`.claude/skills/review-to-issues/SKILL.md`)에, 리뷰 도구와 흐름의 이유는 `Tools/CodeReview/README.md`에 있다. 여기에는 리뷰 밖에서도 지킬 것만 둔다.

- 기능(feat)을 마친 뒤의 리뷰는 그 기능의 diff를 대상으로 한다(diff 리뷰). 폴더 전체 리뷰는 폴더 전체가 대상일 때만 쓴다.
  - 이유: 전체 리뷰는 한 번에 에이전트 토큰이 100만 가까이 든다.
- 리뷰 원본(`Docs/Reviews/날짜-주제.json`)은 리뷰 도구가 낸 그대로 두고 고치지 않는다.
- 열린 리뷰 이슈는 테스트로 재현된 `reproduced`뿐이다. 고치지 않은 `reproduced` 테스트는 저장소에서 계속 실패한다. 열린 `reproduced` 이슈 수만큼 실패가 있는 것이 정상이니, 그 테스트를 지우거나 건너뛰게 하지 않는다.
- 이슈를 고치는 작업은 main에서 `fix/issue-<번호>` 브랜치를 만들어 한다. `reproduced` 테스트가 main에 있어야 하므로, 리뷰 브랜치가 main에 머지된 뒤에 만든다. 고친 커밋의 메시지에 `Fixes #번호`를 적고, 실패하던 테스트가 통과로 바뀌는지 확인한다.
- `not-checked`로 닫은 이슈(`rule`, `cleanup`)를 나중에 고치면 판정 라벨을 `fixed-without-test`로 바꾸고 completed로 닫는다.
- 리뷰 기록이 든 브랜치는 squash나 rebase가 아니라 merge commit으로 합친다. 저장소 설정에서도 squash와 rebase 머지를 막는다.
  - 이유: squash나 rebase로 합치면 지우기 전의 `not-reproduced` 테스트가 든 판정 커밋이 main 기록에서 빠지거나 해시가 바뀌어, 이슈의 링크가 가리키는 커밋을 다시 돌릴 수 없다.
