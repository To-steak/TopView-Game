# TopView Game
[![TopView Game 트레일러](Docs/Image/YouTube_Thumbnail.png)](https://youtu.be/7fyoxN1xmZw)

> [!IMPORTANT]
> 적을 물리치고 강화를 하며 최종적으로 보스를 쓰러뜨리는 탑다운 액션 게임을 기획하고 Claude와 함께 작업하였습니다.
> 2026년 8월에 올려 두고 멈췄던 프로젝트를, 10월 30일부터 Claude Code와 데이터 지향 구조로 다시 짜서 11월 12일 1.0.0을 출시했습니다.

<img src="Docs/Image/smart_display_24dp_FF6666_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"> [TopView Game 트레일러 영상 보러가기 - YouTube](https://youtu.be/7fyoxN1xmZw)

<img src="Docs/Image/smart_display_24dp_FF6666_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"> [TopView Game 플레이 영상 보러가기 - YouTube](https://youtu.be/7VS9hrIHz_A)

<img src="Docs/Image/download_24dp_8B949E_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"> [다운로드 (Release)](https://github.com/To-steak/TopView-Game/releases)


**Engine**  
![Unity](https://img.shields.io/badge/Unity-6000.3-000000?style=flat-square&logo=unity&logoColor=white)
![URP](https://img.shields.io/badge/URP-000000?style=flat-square&logo=unity&logoColor=white)
![C#](https://img.shields.io/badge/C%23-512BD4?style=flat-square&logo=dotnet&logoColor=white)

**AI**  
![Claude Code](https://img.shields.io/badge/Claude_Code-D97757?style=flat-square&logo=claude&logoColor=white)
![Unity CLI](https://img.shields.io/badge/Unity_CLI-000000?style=flat-square&logo=unity&logoColor=white)

**Verification**  
![Roslyn Analyzer](https://img.shields.io/badge/Roslyn_Analyzer-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![Unity Test Framework](https://img.shields.io/badge/Unity_Test_Framework-000000?style=flat-square&logo=unity&logoColor=white)
![GitHub Issues](https://img.shields.io/badge/GitHub_Issues-181717?style=flat-square&logo=github&logoColor=white)


## <a href="#-프로젝트-세-줄-요약"><img src="Docs/Image/summarize_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"></a> 프로젝트 세 줄 요약
<img src="Docs/Image/counter_1_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"> Unity 6으로 탑다운 액션 게임을 팀원 없이 AI(Claude Code)와 함께 기획부터 구현, 트레일러 제작, 배포까지 완성했습니다.

<img src="Docs/Image/counter_2_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"> 기획·설계와 플레이 확인은 제가 맡고 구현과 리뷰는 AI(Claude)에게 맡겼으며, 그 경계를 규칙 문서([CLAUDE.md](/CLAUDE.md#작업-범위))로 정해 AI가 규칙과 어긋나면 [먼저 묻게](/CLAUDE.md#대화) 했습니다.

<img src="Docs/Image/counter_3_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"> AI가 쓴 코드는 프로젝트 규칙으로 만든 정적 분석기([Roslyn](/Tools/TopView.Analyzers/README.md)), 코드를 작성한 세션과 다른 새 세션의 AI 리뷰([Claude Code Workflow](/Tools/CodeReview/README.md)), 버그 지적의 재현 테스트([Unity Test Framework](/Tools/Testing/README.md))를 거쳐 확인하고, 모든 지적을 판정과 함께 이슈로 기록해 재현된 버그만 열어 두었습니다([GitHub Issues](/Tools/ReviewIssues/README.md)).

## <a href="#-개발-과정"><img src="Docs/Image/history_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"></a> 개발 과정
자세한 이야기는 Notion에 있습니다.

<details>
<summary><b>AI와 개발자의 역할 분담</b></summary>

**개발자**
- 프로젝트의 코드 구조를 AI와 함께 설계했습니다([아키텍처 규칙](/.claude/rules/architecture.md)).
- 씬, 프리팹, 에셋 같은 유니티 에디터 작업을 맡았습니다.
- 직접 플레이해 보고 수치 편집 창으로 게임 내 수치를 조정해 밸런스를 잡았습니다.
- AI의 작업 범위와 규칙을 문서로 정해, 규칙과 어긋나는 지시는 AI가 먼저 되묻게 했습니다([작업 범위](/CLAUDE.md#작업-범위), [되묻기 규칙](/CLAUDE.md#대화), [코드 규칙](/.claude/rules/csharp-style.md)).

**AI(Claude Code)**
- 게임 코드를 작성하고 리뷰했습니다.
- 제가 요청한 대로 [수치 편집 창](/Project/Assets/Scripts/Editor/DefinitionEditorWindow.cs#L6-L7), [타일 배치 도구](/Project/Assets/Scripts/Editor/LevelTilePainterWindow.cs#L8-L17) 같은 Custom Editor를 만들었습니다.
- Unity CLI로 컴파일 오류와 테스트 결과를 직접 확인하고, GitHub CLI로 리뷰 지적을 이슈로 등록했습니다([AI가 일할 수 있는 환경](#-ai가-일할-수-있는-환경)).

<img src="Docs/Image/Definition%20Editor.png" width="49%" alt="수치 편집 창"> <img src="Docs/Image/Level%20Tile%20Painter.png" width="49%" alt="타일 배치 도구">

</details>

<details>
<summary><b>AI의 코드 리뷰</b></summary>

- Claude Code의 기본 `/code-review`를 써 보고, 영역을 나눠 에이전트가 읽고 지적마다 실패 시나리오를 적는 구조를 참고했습니다.
- 리뷰의 지적은 추측이라고 보고, 버그 지적은 테스트를 써서 재현되는지, 기획 의도일 수 있는 지적은 제가 판단해 판정하는 흐름을 더했습니다.
- 모든 지적을 판정 라벨과 함께 GitHub 이슈로 남기고, 테스트로 재현된 버그만 열어 둡니다.
- 이 흐름을 설계하고 구현은 AI에게 맡겨 [리뷰 워크플로](/Tools/CodeReview/README.md), [이슈 등록 도구](/Tools/ReviewIssues/README.md), 두 도구를 묶는 스킬([`/review-to-issues`](/.claude/skills/review-to-issues/SKILL.md))을 만들었습니다.
- 버그를 심어 리뷰 도구를 측정해 보니([전체 리뷰 측정](/Docs/ReviewBenchmark/2026-11-15-planted-bugs/README.md)) 재현율은 평균 86.7%였지만, 폴더 전체를 읽는 방식이라 한 번에 에이전트 토큰이 약 100만 들었습니다.
- 바뀐 코드만 읽는 diff 리뷰 워크플로를 새로 만들어 토큰을 약 8만(12분의 1)으로 줄였고, 심은 버그 6개는 세 번 모두 찾았습니다([diff 리뷰 측정](/Docs/ReviewBenchmark/2026-11-16-critical-hit-diff/README.md)). 단, diff 73줄에 버그가 모두 diff 안에 있는 조건이라 전체 리뷰와 재현율을 직접 비교할 수는 없습니다.

![판정 라벨과 함께 등록된 리뷰 지적 이슈 목록](Docs/Image/Review_Issues.png)

</details>

<details>
<summary><b>문제를 나누고 정리한 방법</b></summary>

- 기능 하나를 구현 → 리뷰 → 판정 → 이슈 → 수정의 다섯 단계로 나눴습니다.
- 리뷰의 지적은 Claude의 추측으로 여기고, 버그 지적은 테스트를 써서 재현되는지 먼저 판정했습니다.
- 판정과 상관없이 모든 지적을 이슈로 남기고, 재현된 버그는 수정 브랜치에서 고쳐 PR로 합쳤습니다. 커밋마다 `Fixes #번호`로 이슈를 닫았습니다.
- 지금까지 지적 49개를 판정했습니다(재현 23, 테스트 없이 고침 12, 의도된 사항 8, 확인 안 함 4, 재현 안 됨 2). 재현된 23개는 모두 고쳤습니다.

![출처, 위치, 판정이 담긴 리뷰 지적 이슈](Docs/Image/Review_Issue_Detail.png)

</details>

<details>
<summary><b>새로 배워서 적용한 내용</b></summary>

- **Roslyn 분석기**: 코드 규칙을 Unity Console의 경고로 띄우려고 처음 도입했습니다. 규칙과 그 동작을 보여 주는 예시 문서는 제가 정하고 분석기 구현은 AI에게 맡겨, 규칙 25개를 만들었습니다. Unity 6000.3의 컴파일러가 Roslyn 4.3이라 그 버전에 맞춰 빌드했습니다([분석기](/Tools/TopView.Analyzers/README.md)).
- **Unity Test Framework**: 리뷰 지적을 재현하려고 처음 도입했습니다. 열려 있는 에디터를 끊지 않으려고 PlayMode 테스트는 별도 워크 트리에서 배치 모드로 돌리게 했습니다([테스트 실행 도구](/Tools/Testing/README.md)).
- **Claude Code Workflow**: 기본 `/code-review`의 구조를 참고해 리뷰 흐름과 결과 형식을 정하고, 스크립트 구현은 AI에게 맡겼습니다. 버그를 심어 성능을 측정한 뒤 diff 리뷰로 바꿨습니다([리뷰 워크플로](/Tools/CodeReview/README.md)).

</details>

<details>
<summary><b>트레일러 영상 제작 과정</b></summary>

- 시네마틱 영상도 게임 세계관을 보여 주고 흥미를 끄는 중요한 장치라고 보고, 해 보고 싶었던 트레일러 영상 제작을 시도했습니다.
- Unity Timeline으로 컷마다 연출을 짜고, Unity Recorder로 녹화했습니다.
- ArtCraft의 영상 편집기를 CLI 제어 모드와 MCP 커넥터로 Claude Code에 연결했습니다. 컷 구성과 연출은 제가 정하고, 컷 배치와 편집 명령은 AI가 실행했습니다.
- 3일 만에 1분 20초 트레일러를 완성해 YouTube에 올렸습니다([트레일러](https://youtu.be/7fyoxN1xmZw)).

![Timeline으로 컷을 재생하는 화면](Docs/Image/Trailer_Timeline.gif)

</details>

## <a href="#-ai가-일할-수-있는-환경"><img src="Docs/Image/smart_toy_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"></a> AI가 일할 수 있는 환경
AI가 일을 맡을 때마다 결과를 사람에게 묻지 않고, 스스로 확인하고 고칠 수 있게 도구를 연결했습니다.

| 도구 | AI가 스스로 할 수 있게 된 일 |
|---|---|
| [`CLAUDE.md`](/CLAUDE.md), [`.claude/rules/`](/.claude/rules/) | 매번 설명하지 않아도 프로젝트 규칙과 작업 범위를 알고 시작합니다 |
| Unity CLI (`unity recompile`, [컴파일 확인](/Tools/Testing/run_tests.sh#L69-L73)) | 코드를 고친 뒤 열려 있는 에디터에서 컴파일 오류와 분석기 경고를 확인하고 고칩니다 |
| [테스트 실행 도구](/Tools/Testing/README.md) (`run_tests.sh`) | EditMode/PlayMode 테스트를 돌리고, 결과를 JSON으로 읽어 판정에 씁니다 |
| PlayMode 전용 워크 트리 ([동기화와 실행](/Tools/Testing/run_tests.sh#L86-L106)) | 원본 코드를 다른 폴더로 맞춘 뒤 배치 모드 에디터로 PlayMode 테스트를 돌려, 사용자가 작업 중인 에디터를 끊지 않습니다 |
| 스킬 [`/review-to-issues`](/.claude/skills/review-to-issues/SKILL.md) | 리뷰 → 판정 → 판정 커밋 → 이슈 등록이라는 반복 절차를 정해진 순서대로 수행합니다 |
| GitHub CLI (`gh`) | 리뷰 지적을 판정 라벨과 함께 이슈로 등록합니다([이슈 등록 도구](/Tools/ReviewIssues/README.md)) |
| [ArtCraft](https://github.com/storytold/artcraft) (CLI 제어 모드, MCP 커넥터) | 트레일러 편집과 텍스처·썸네일 같은 이미지 가공 명령을 실행합니다 |


![AI가 테스트 실행 도구로 EditMode 테스트를 돌린 결과](Docs/Image/Run_Tests.png)

AI가 하지 않는 일도 함께 정했습니다. 에디터를 열거나 Play 모드에 들어가지 않고, 씬·프리팹·에셋·`.meta`는 건드리지 않습니다([에디터와 파일](/CLAUDE.md#에디터와-파일), [작업 범위](/CLAUDE.md#작업-범위)). 에디터 상태는 사용자의 작업을 끊을 수 있어 사람이 정합니다.

## <a href="#-검증-체계"><img src="Docs/Image/verified_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"></a> 검증 체계
AI가 쓴 코드는 네 단계를 거칩니다. 앞 단계가 놓친 것을 다음 단계가 잡고, 각 단계의 결과는 저장소에 남습니다.

| 단계 | 언제 | 무엇을 확인하나 | 근거 |
|---|---|---|---|
| 1. 정적 분석기 | 컴파일할 때마다 | 코드 규칙 25개를 Unity Console 경고로 띄웁니다. 규칙의 동작은 예시 문서가 정하고, 그 예시가 곧 분석기의 테스트입니다 | [분석기](/Tools/TopView.Analyzers/README.md), [예시 문서](/Tools/TopView.Analyzers.Tests/Examples/) |
| 2. 코드 리뷰 | 기능을 마친 뒤 | 코드를 작성한 세션과 다른 새 세션의 에이전트들이 바뀐 코드를 영역별로 읽고, 지적마다 실패 상황을 적습니다 | [리뷰 워크플로](/Tools/CodeReview/README.md), [리뷰 원본](/Docs/Reviews/) |
| 3. 판정 | 지적마다 | 리뷰의 지적은 추측으로 봅니다. 버그 지적은 고치기 전 코드에서 실패하는 테스트를 써서 실제 버그인지 판정하고, 기획 의도일 수 있는 지적은 제가 판단하며, 규칙·정리·불일치 지적은 테스트 없이 기록합니다 | [테스트 실행 도구](/Tools/Testing/README.md), [테스트](/Project/Assets/Scripts/Tests/) |
| 4. 이슈와 수정 | 판정한 뒤 | 모든 지적을 판정 라벨과 함께 이슈로 남기고, 재현된 버그만 열어 둡니다. 실패하던 테스트가 통과로 바뀌면 고친 것으로 봅니다 | [이슈 등록 도구](/Tools/ReviewIssues/README.md), [리뷰 이슈](https://github.com/To-steak/TopView-Game/issues?q=label%3Areview-finding) |

![규칙을 어긴 코드에 Unity Console이 띄운 분석기 경고 TV0002](Docs/Image/Analyzer_Warning.png)

**판정 규칙**
- 재현된 버그의 테스트는 고칠 때까지 실패한 채 저장소에 남깁니다. 지우거나 건너뛰게 하지 않아서, 열린 이슈 수만큼 실패하는 테스트가 있는 것이 정상입니다.
- 재현되지 않은 지적의 테스트는 판정 커밋에 남긴 뒤 다음 커밋에서 지웁니다. 판정 커밋을 다시 체크아웃하면 그때의 판정을 그대로 다시 돌려 볼 수 있습니다.
- 고친 커밋에는 `Fixes #번호`를 적고, 실패하던 테스트가 통과하는지 확인합니다.

**리뷰가 놓치는 것**
버그를 심어 리뷰 도구를 측정했을 때, 한 틱 차이로 생기는 버그는 세 번 모두 놓쳤습니다([전체 리뷰 측정](/Docs/ReviewBenchmark/2026-11-15-planted-bugs/README.md)). 이런 버그는 사람도 에디터에서 재현할 수 없어서 테스트로만 확인합니다.

## <a href="#-기술-구조"><img src="Docs/Image/account_tree_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"></a> 기술 구조
DOTS 패키지 없이 순수 C#으로 데이터 지향 구조를 만들었습니다. 데이터는 컴포넌트에, 로직은 상태 없는 System에 두는 ECS의 원칙을 따르되, 저장은 모든 컴포넌트를 필드로 가진 엔티티 struct 하나를 고정 배열에 담는 단순한 방식으로 했습니다. 칸이 가진 컴포넌트는 마스크로 표시합니다. 자세한 규칙과 이유는 [아키텍처 규칙](/.claude/rules/architecture.md)에 있습니다.

```
입력 → Command → System(FixedUpdate) → Entity 변경 → Record
                                                       ↓
              View ← ViewSystem / Presenter(LateUpdate) ← Entity, Record
```

**주요 설계**
- **Logic과 View 분리**: 게임 상태는 FixedUpdate(50Hz)의 System에서만 바꾸고, 화면은 LateUpdate에서 그 상태를 읽어 보간해 그립니다. 프레임레이트가 달라도 판정과 결과가 같습니다.
- **이벤트 대신 기록**: C# 이벤트를 쓰지 않고, 그 틱에 일어난 일을 기록(`~Record`)으로 남겨 View가 정해진 시점에 읽습니다. 호출 흐름이 코드에 그대로 보입니다(UI 버튼 입력만 예외).
- **실행 순서는 한 곳에서**: Unity의 Update 계열 함수는 App만 받고, 한 판 안의 틱 순서와 호출 순서는 World가 정합니다.
- **재현할 수 있는 판**: 시간은 정수 틱으로, 랜덤은 World의 시드 고정 난수로 다뤄 같은 입력이면 같은 결과가 나옵니다. 그래서 한 틱 차이 버그도 테스트로 다시 만들 수 있습니다.
- **데이터와 수치 분리**: 수치는 ScriptableObject(`~Definition`)에 두고 에디터에서 조정합니다. 생성할 때 칸의 컴포넌트로 복사해, 매 틱 로직은 컴포넌트만 읽습니다.
- **할당 줄이기**: 게임 중에는 `Instantiate`/`Destroy`를 반복하지 않고 View 오브젝트를 풀에서 꺼내 씁니다.

**어셈블리 구성**

화살표는 참조 방향입니다. Presentation과 UserInterface는 Systems를 참조하지 못해서, View가 게임 상태를 바꾸는 코드는 컴파일되지 않습니다.

```mermaid
graph TD
    subgraph RunLayer["실행: 언제 무엇을 부를지 정함"]
        Core["Core (App, World)"]
    end
    subgraph LogicLayer["로직: 게임 상태를 바꾸는 유일한 곳"]
        Systems
    end
    subgraph ViewLayer["화면: 상태를 읽어 그리기만 함"]
        Presentation
        UserInterface
    end
    subgraph DataLayer["데이터: 게임에 무엇이 있는지"]
        Entity["Entity (컴포넌트, 요청, 기록, 명령)"]
        Definitions
        Authoring
        Persistence
    end
    Core --> Systems
    Core --> Presentation
    Core --> UserInterface
    Core --> Authoring
    Core --> Persistence
    Systems --> Entity
    Presentation --> Entity
    UserInterface --> Entity
    Authoring --> Definitions
    Authoring --> Entity
    Definitions --> Entity
    Persistence --> Entity
```

**실행**: 언제 무엇을 부를지 정합니다
- `Core`: App(씬 전환, 설정, 배경음악 같은 앱 수명의 일)과 World(한 판의 데이터와 틱·호출 순서)

**로직**: 게임 상태를 바꾸는 유일한 곳입니다
- `Systems`: System과 값 계산 도구(Utility)

**화면**: 상태를 읽어 그리기만 합니다
- `Presentation`: 게임 세계의 연출(ViewSystem, View, 오브젝트 풀)
- `UserInterface`: MVP 구조의 화면 UI(View, Presenter)

**데이터**: 게임에 무엇이 있는지 담습니다
- `Entity`: 엔티티, 컴포넌트, 요청, 기록, 명령. 다른 프로젝트 어셈블리를 참조하지 않습니다
- `Definitions`: 에디터에서 조정하는 수치(ScriptableObject)
- `Authoring`: 씬에 놓인 엔티티의 종류와 위치
- `Persistence`: 설정과 기록 저장

**도구**: 게임 밖에서 씁니다
- `Editor`: 수치 편집 창, 타일 배치 도구 같은 에디터 도구. 빌드에 포함되지 않습니다
- `Trailer`: 트레일러 촬영 씬 전용 연출. 게임 어셈블리를 참조하지 않습니다

사용한 패키지: Input System · Cinemachine · DOTween · TextMeshPro · uGUI

## <a href="#-저장소-구성"><img src="Docs/Image/folder_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"></a> 저장소 구성
```
TopView-Game/
├── Project/          Unity 프로젝트
├── Tools/            정적 분석기, 리뷰·이슈·테스트 도구
├── Docs/             리뷰 원본, 리뷰 도구 측정, 이미지
├── .claude/          AI 작업 규칙과 스킬
├── .github/          이슈 폼
└── CLAUDE.md         AI 작업 범위와 프로젝트 규칙
```

**게임**
- [`Project/Assets/Scripts/`](/Project/Assets/Scripts/): 게임 코드. 폴더 구성은 [기술 구조](#-기술-구조)와 같습니다
- [`Project/Assets/Scripts/Tests/`](/Project/Assets/Scripts/Tests/): EditMode, PlayMode 테스트

**AI 작업 규칙**
- [`CLAUDE.md`](/CLAUDE.md): 작업 범위, 대화 규칙, 프로젝트 값
- [`.claude/rules/`](/.claude/rules/): 아키텍처 규칙, C# 코드 규칙
- [`.claude/skills/review-to-issues/`](/.claude/skills/review-to-issues/SKILL.md): 리뷰부터 이슈 등록까지의 절차

**도구**
- [`Tools/TopView.Analyzers/`](/Tools/TopView.Analyzers/README.md): 정적 분석기. 테스트와 규칙별 예시 문서는 [`Tools/TopView.Analyzers.Tests/`](/Tools/TopView.Analyzers.Tests/Examples/)
- [`Tools/CodeReview/`](/Tools/CodeReview/README.md): 리뷰 워크플로
- [`Tools/ReviewIssues/`](/Tools/ReviewIssues/README.md): 이슈 등록 도구
- [`Tools/Testing/`](/Tools/Testing/README.md): 테스트 실행 도구
- [`.github/ISSUE_TEMPLATE/`](/.github/ISSUE_TEMPLATE/review-finding.yml): 리뷰 지적 이슈 폼

**기록**
- [`Docs/Reviews/`](/Docs/Reviews/): 리뷰 도구가 낸 리뷰 원본(고치지 않음)
- [`Docs/ReviewBenchmark/`](/Docs/ReviewBenchmark/): 버그를 심어 리뷰 도구를 측정한 기록

## <a href="#-실행-방법"><img src="Docs/Image/sports_esports_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"></a> 실행 방법
**플레이** (Windows 전용)
1. [Release](https://github.com/To-steak/TopView-Game/releases/latest)에서 `TopView_Game.zip`을 받아 압축을 풉니다.
2. 압축을 푼 폴더의 실행 파일을 엽니다.

**조작**

| 키 | 동작 |
|---|---|
| W A S D | 이동 |
| Left Shift (누르는 동안) | 달리기 |
| Space | 점프 |
| 마우스 이동 | 조준 |
| 마우스 왼쪽 | 공격 |
| 마우스 오른쪽 | 회피 |
| 1 ~ 4 | 무기 바꾸기 |
| R | 재장전 |
| X | 투척 조준, 재장전 취소 |
| Q / E | 카메라 회전 |
| Tab | 강화 |
| Esc | 일시정지 |

![이동, 점프, 카메라 회전, 달리기를 하는 플레이 장면](Docs/Image/Gameplay.gif)

**프로젝트 열기**
- Unity 6000.3.27f2 (URP)에서 `Project/` 폴더를 엽니다.
- 에셋 스토어 에셋(`Project/Assets/Local Assets/`)과 스카이박스 패키지는 재배포할 수 없어 저장소에 넣지 않았습니다. 저장소만 받아서는 모델, 사운드, 폰트가 빠진 채로 열립니다. 플레이는 Release의 빌드로 해 주세요.

## <a href="#-license"><img src="Docs/Image/license_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"></a> License
프로젝트에 사용한 외부 에셋과 라이선스입니다. 저장소에 넣지 않은 에셋의 라이선스 파일은 [`Docs/License/`](/Docs/License/)에 복사해 두었습니다.

**CC0 (저장소에 포함)**

[`Project/Assets/CC0/`](/Project/Assets/CC0/)에 있고, 폴더마다 라이선스 파일이 함께 있습니다.
- Kenney: [Nature Kit](/Project/Assets/CC0/Nature%20Kit%20%282.1%29/License.txt), [Crosshair Pack](/Project/Assets/CC0/kenney_crosshair-pack/License.txt), [Cursor Pixel Pack](/Project/Assets/CC0/kenney_cursor-pixel-pack/License.txt), [Input Prompts](/Project/Assets/CC0/kenney_input-prompts_1.5/License.txt), [Interface Sounds](/Project/Assets/CC0/kenney_interface-sounds/License.txt), [UI Pack - Pixel Adventure](/Project/Assets/CC0/kenney_ui-pack-pixel-adventure/License.txt) - [kenney.nl](https://www.kenney.nl)
- Quaternius: [Ultimate Nature Pack](/Project/Assets/CC0/Ultimate%20Nature%20Pack%20by%20Quaternius/License.txt)

**Unity Asset Store (저장소에 포함하지 않음)**

[Unity Asset Store Terms of Service and EULA](https://unity.com/legal/as-terms)를 따릅니다.
- [Quarter View 3D Action Assets Pack](https://assetstore.unity.com/packages/3d/characters/quarter-view-3d-action-assets-pack-188720) - [Goldmetal](https://assetstore.unity.com/publishers/52011)
- [Super Simple Sky](https://assetstore.unity.com/packages/2d/textures-materials/sky/super-simple-sky-dynamic-procedural-skybox-shader-day-night-cycl-210177) - [OccaSoftware](https://assetstore.unity.com/publishers/52431) ([라이선스](/Docs/License/SuperSimpleSkybox/LICENSE.md))
- [The 8-bit Jukebox Lite](https://assetstore.unity.com/packages/p/the-8-bit-jukebox-lite-music-pack-70436) - [Cyberleaf Studio](https://assetstore.unity.com/publishers/963), 작곡 Andrea Baroni ([andreabaroni.com](https://andreabaroni.com))
- [Free Weapon Sound Effects](https://assetstore.unity.com/packages/audio/sound-fx/weapons/free-weapon-sound-effects-388474) - [SoundLab_1](https://assetstore.unity.com/publishers/26808)
- [Shooting Sound](https://assetstore.unity.com/packages/audio/sound-fx/shooting-sound-177096) - [B.G.M](https://assetstore.unity.com/publishers/9381) ([라이선스](/Docs/License/ShootingSound/ReadMe.txt))

**그 밖의 에셋 (저장소에 포함하지 않음)**
- DNFBitBit 폰트 - NEOPLE Inc., SIL Open Font License 1.1 ([라이선스](/Docs/License/DNFBitBit/OFL.txt))
- 효과음 - [효과음 라보(効果音ラボ)](https://soundeffect-lab.info/), 사이트 이용 약관을 따릅니다
