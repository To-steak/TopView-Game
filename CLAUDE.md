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
Claude는 코드만 쓴다. Unity 프로젝트 안에서 만들고 수정하는 파일은 다음뿐이다.
- 스크립트 루트 아래의 `.cs`, `.asmdef`, `.asmref`
- 정적 분석기를 빌드해 `Project/Assets/Analyzers/`에 복사하는 `TopView.Analyzers.dll` (`.meta`는 사용자가 다룬다)

Unity 프로젝트 밖에서는 정적 분석기 소스 `Tools/TopView.Analyzers/`와 그 테스트 `Tools/TopView.Analyzers.Tests/`를 만들고 수정한다.
- 이유: 분석기는 Unity가 아닌 .NET 라이브러리로 빌드해야 해서, 소스를 `Assets` 안에 두면 Unity가 게임 코드로 컴파일하려 한다.
- 빌드: `dotnet build Tools/TopView.Analyzers -c Release` (빌드가 DLL을 `Project/Assets/Analyzers/`로 복사한다)
- 분석기를 고치면 `dotnet test Tools/TopView.Analyzers.Tests`로 확인한다. 규칙을 더하거나 바꾸면 먼저 그 규칙의 예시 문서(`Tools/TopView.Analyzers.Tests/Examples/`)를 고치고, 분석기를 예시에 맞춘다.
  - 이유: 사용자는 분석기 코드(Roslyn)가 아니라 예시 문서로 규칙의 동작을 확인한다. 예시가 곧 테스트라 문서와 분석기가 어긋나지 않는다.

작업의 근거가 되는 문서는 `Docs/` 아래에 만들고 수정한다.
- 이유: 이슈, 커밋, 테스트가 무엇을 근거로 했는지 저장소에서 따라갈 수 있어야 한다. 대화나 임시 폴더에만 있으면 사라진다.
- 코드 리뷰의 결과는 `Docs/Reviews/`에 리뷰마다 `날짜-주제.json` 하나로 둔다. 리뷰 도구가 낸 원본이고, 고치지 않는다.
  - 원본에는 리뷰한 날짜, 커밋, 상황 설명, 영역 목록(실행할 때 넘긴 값)과 지적이 함께 들어 있다.
  - 예외: 범용 도구를 만들기 전의 리뷰(`2026-11-14-game-code-review`)는 원본에 커밋이 없어, 이슈 도구에 `--commit 45ba8812`를 준다.
  - 이유: 원본만 보고 무엇을 어떤 범위로 리뷰했는지와 지적의 출처를 알 수 있다.

코드 리뷰는 `Tools/CodeReview/review.workflow.js`를 워크플로로 실행해서 한다.
- 스크립트는 바뀌지 않는 부분(결과 형식인 JSON 스키마, 공통 지침)만 가진다. 리뷰마다 바뀌는 값(날짜, 커밋, 상황 설명, 영역 목록)은 실행할 때 `args`로 넘기고, 넘기기 전에 사용자에게 보여 준다.
  - 이유: 범위와 상황이 스크립트에 박혀 있으면 리뷰마다 스크립트를 고치거나 복사해야 하고, 무엇이 바뀌었는지 흐려진다.
- 결과 형식을 바꾸면 이슈 폼과 이슈 도구(`Tools/ReviewIssues/`)도 함께 바꾼다. 셋이 같은 필드 목록을 쓴다.

코드 리뷰의 지적은 다음 순서로 처리한다. 리뷰부터 2까지는 스킬 `/review-to-issues`(`.claude/skills/review-to-issues/`)가 한다.
1. `bug`, `inconsistency` 지적마다 그 버그로 실패하는 테스트(EditMode 또는 PlayMode)를 쓰고 `Tools/Testing/run_tests.sh`로 돌린다. 통과하면 버그가 아니므로 그 테스트를 지우고 이슈로 열지 않는다.
2. 원본 JSON의 지적과 실패한 테스트로 이슈를 등록한다. 도구는 `Tools/ReviewIssues/`이고, 실패 메시지는 테스트 결과 파일에서 읽는다. 본문은 이슈 폼 `.github/ISSUE_TEMPLATE/review-finding.yml`과 같은 모양이다. 본문을 따로 보여 주거나 확인받지 않고 등록한다.
3. 고친 커밋의 메시지에 `Fixes #번호`를 적어, 머지되면 이슈가 닫히게 한다.
- 이유: 사람이 읽는 단위가 버그 하나인 이슈가 되고, 이슈마다 출처(원본 JSON의 지적)와 증거(실패한 테스트)가 붙는다. 재현하지 못한 지적은 이슈가 되지 않아 추측이 할 일로 섞이지 않는다.
- 한 틱 차이로 생기는 버그는 사람이 에디터에서 재현할 수 없으므로, 확인 방법은 테스트뿐이다.
- 리뷰는 보통 기능(feat)을 마친 뒤 그 범위를 대상으로 한다. 2026-11-14의 전체 리뷰는 1.0.0 출시 뒤 쌓인 버그를 한 번에 찾은 예외다.

리뷰 도구는 `Tools/CodeReview/`에, 테스트 실행 도구는 `Tools/Testing/`에, 이슈 폼은 `.github/ISSUE_TEMPLATE/`에, 이슈 본문을 만드는 도구는 `Tools/ReviewIssues/`에, 프로젝트 스킬은 `.claude/skills/`에 만들고 수정한다.

테스트는 `sh Tools/Testing/run_tests.sh editmode|playmode [필터]`로 돌린다. 결과는 `Project/Temp/TestResults/<mode>.json`에 남는다.
- editmode는 사용자가 연 에디터에서 다시 컴파일하고(컴파일 오류, 분석기 경고를 보여 준다) 돌린다. 연결된 에디터가 없으면 돌리지 않는다.
- playmode는 아래 예외의 워크 트리를 원본에 맞춘 뒤 배치 모드로 돌린다.

나머지(씬, 프리팹, 에셋, 설정, `.meta`)는 사용자가 에디터에서 다룬다.
- Claude는 에디터를 직접 조작하지 않는다. 에디터에서 할 일(씬, 프리팹, 에셋, 설정)이 생기면 응답에 구체적으로 적는다.
- 코드를 고친 뒤에는 `unity recompile --project-path Project`로 실행 중인 에디터의 컴파일 오류를 확인하고, 오류가 있으면 고친다. 연결된 에디터가 없으면(`unity status`) 확인하지 못했다고 알리고, 에디터를 띄우거나 Play 모드에 들어가지 않는다.
  - 이유: 컴파일 오류를 Claude가 먼저 잡으면 사용자가 확인하는 일이 동작 검토로 줄어든다. 에디터 상태(열기, Play 모드)는 사용자의 작업을 끊을 수 있어 사용자가 정한다.
- 예외: PlayMode 테스트는 테스트 전용 워크 트리 `../TopView-Game-Test`에서 배치 모드 에디터로 돌린다(`unity test "C:/Users/duddn/Repository/TopView-Game-Test/Project" --mode PlayMode`, 상대 경로를 주면 같은 이름의 다른 프로젝트와 헷갈려 실패한다). 사용자가 연 에디터는 건드리지 않는다. 오래 걸리므로 백그라운드로 돌린다.
  - 이유: 다른 폴더의 배치 모드 에디터는 사용자의 에디터와 씬을 끊지 않는다. 같은 프로젝트 폴더는 에디터 둘이 동시에 열 수 없다.
  - 워크 트리는 지우지 않고 계속 쓴다. `Library/`는 처음 한 번 원본에서 복사했고, 그 뒤로는 워크 트리의 Unity가 자기 `Assets/`를 보고 맞춘다.
  - 테스트 전에 git이 무시하는 에셋 폴더(`Project/Assets/Local Assets/`, `Project/Packages/com.occasoftware.super-simple-skybox/`)와 스크립트 폴더를 원본에서 `robocopy /MIR`로 맞춘다. 커밋하지 않은 코드도 함께 맞춰진다. `run_tests.sh playmode`가 이 일을 한다.
- **파일을 옮길 때는 짝인 `.meta`와 함께 `git mv`로 옮긴다.** 새 폴더의 `.meta`는 만들지 않고 Unity가 만들게 둔다. 파일 이름은 바꾸지 않는다. 클래스 이름을 바꿔야 하면 사용자가 에디터에서 파일 이름을 먼저 바꾼 뒤 코드를 고친다.
  - 이유: GUID는 `.meta`에 있어서, `.cs`만 옮기거나 `.meta`를 새로 만들면 GUID가 끊어져 에셋과 씬의 참조가 사라진다. 짝을 함께 옮기면 GUID가 그대로 따라간다.
