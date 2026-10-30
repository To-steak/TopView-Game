# CLAUDE.md

## 프로젝트 값
`.claude/rules/`의 규칙은 프로젝트에 묶이지 않은 원칙만 담는다. 이 프로젝트에서의 구체 값은 여기에 둔다.

- 엔진: Unity 6 (6000.3.11f1), URP. 타겟 플랫폼은 PC다.
- Unity 프로젝트 루트: `Project/`
- 스크립트 루트: `Project/Assets/Scripts/`
- 어셈블리·네임스페이스 접두사: `TopView`
- Definition Catalog: `Project/Assets/Resources/Definition Catalog.asset` (`Resources.Load` 경로는 `"Definition Catalog"`)
- Fixed Timestep: 0.02초 (50Hz)
- 외부 패키지 참조
  - `TopView.Core` → `Unity.InputSystem`
  - `TopView.Presentation` → `Unity.Cinemachine`
  - `TopView.UserInterface` → `UnityEngine.UI`, `Unity.TextMeshPro`, `DOTween.Modules`
  - `TopView.Editor` → `UnityEngine.UI`

## 대화
- 응답은 항상 한국어로 한다.
- 질문에는 답만 한다. 파일 수정은 작업을 요청받았을 때만 한다.
- 새 시스템이나 규칙을 도입해야 하면 코드 작성 전에 묻는다.
- 설계에 값이 없는 작은 수치나 동작은 정해서 진행하되, 응답에 무엇을 정했는지 알린다.

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

나머지(씬, 프리팹, 에셋, 설정, `.meta`)는 사용자가 에디터에서 다룬다.
- Claude는 에디터를 실행할 수 없다. 에디터에서 할 일이 생기면 응답에 구체적으로 적는다.
- 컴파일은 사용자가 에디터에서 확인한다. 테스트 코드는 작성하지 않는다.
- **파일을 옮기거나 이름을 바꾸지 않는다.** 에디터 밖에서 옮기면 `.meta`의 GUID가 끊어진다. 꼭 필요하면 어느 파일을 어디로 옮길지 응답에 적고 사용자에게 에디터에서 해 달라고 한다. 클래스 이름을 바꿔야 하면 사용자가 에디터에서 파일 이름을 먼저 바꾼 뒤 코드를 고친다.
