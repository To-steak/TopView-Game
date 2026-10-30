---
paths:
  - "Project/Assets/Scripts/**/*.cs"
  - "Project/Assets/Scripts/**/*.asmdef"
  - "Project/Assets/Scripts/**/*.asmref"
---

# 아키텍처 규칙

ECS 패키지(DOTS)는 쓰지 않는다. "System은 함수, 상태는 데이터"라는 철학만 순수 C#으로 따른다.

규칙마다 `이유:` 줄은 그 규칙이 없으면 무엇이 깨지는지를 적는다. 이유가 성립하지 않는 상황이면 규칙을 어기기 전에 먼저 묻는다.

## Logic / View 분리
- **Logic은 FixedUpdate에서만 실행한다.** 게임 상태를 바꾸는 코드는 System의 `Tick` 안에만 둔다.
  - 이유: 프레임레이트에 따라 판정과 결과가 달라지지 않는다.
- **View는 Update / LateUpdate에서만 실행한다.** 애니메이션, SFX, VFX, UI, 카메라가 View다.
  - 이유: 화면은 프레임마다 그리되, 그리는 일이 게임 결과를 바꾸지 않는다.
- Logic은 View를 참조하지 않는다. 그 틱에 일어난 일은 프레임 기록 목록(`List<~Record>`)에 남긴다.
  - 이유: View 없이도 Logic이 돈다. 사건을 데이터로 남기면 누가 언제 읽을지가 Runner의 호출 순서에 고정된다.
- View는 게임 상태를 읽기만 한다. 입력은 명령 버퍼(`~Command`)를 거친다.
  - 이유: 상태를 바꾸는 곳이 System뿐이라, 상태가 틀렸을 때 System만 보면 된다.
- **Transform은 View만 쓴다.** Logic은 위치를 데이터(`previousPosition`, `currentPosition`)로 소유하고, View가 보간해 반영한다.
  - 이유: 위치의 정본이 둘이면 판정 위치와 보이는 위치가 어긋난다.
- 판정 시점은 Logic의 타이머가 정한다. 애니메이션 이벤트로 게임 상태를 바꾸지 않는다.
  - 이유: 애니메이션 이벤트는 블렌딩, 컬링, 프레임레이트에 따라 늦거나 빠지거나 두 번 불린다.
- Unity 물리는 질의로만 쓴다. 지형 등 씬에 배치한 정적 오브젝트에 Collider를 붙이고, System이 `Tick` 안에서 `Physics` 질의(Raycast, SphereCast, Overlap 등)를 호출해 그 결과로 위치 데이터를 고친다.
  - 이유: 질의는 호출 시점과 결과가 System 코드 안에서 보인다.
- NavMesh도 질의로만 쓴다. System이 `Tick` 안에서 `NavMesh` 질의(CalculatePath, SamplePosition 등)를 호출해 경로를 얻고, 위치 데이터는 System이 직접 옮긴다. NavMeshAgent는 쓰지 않는다.
  - 이유: NavMeshAgent는 자체 업데이트에서 Transform을 옮기고 회피를 계산해, Tick 밖의 시점에 위치가 바뀐다.
- Rigidbody, 물리 시뮬레이션, 충돌 콜백(`OnCollision~`, `OnTrigger~`)은 쓰지 않는다. 엔티티는 Collider를 갖지 않으며, 엔티티끼리의 충돌과 범위 판정은 Logic이 직접 계산한다.
  - 이유: 물리 시뮬레이션은 Tick 밖의 시점에 위치를 바꾸고, 충돌 콜백은 이벤트처럼 호출 순서가 숨는다.

## World / Runner
- **World**는 한 판의 데이터(엔티티 저장소, 기록 목록, 요청 큐, 명령 버퍼, 난수 생성기)를 모아 두는 컨테이너다. Core에 있고 Runner만 쓴다. 판마다 `new World()`로 새로 만든다.
  - 이유: 한 판의 상태가 객체 하나에 있으면 새 판을 시작할 때 이전 판의 값이 남지 않는다.
- **Runner가 World에서 필요한 데이터를 꺼내 System, ViewSystem, Presenter에 넘긴다.** 이들은 World를 받지 않는다.
  - 이유: World를 통째로 받으면 시그니처만 보고 무엇을 읽고 쓰는지 알 수 없다.
- 틱 순서와 호출 순서는 Runner가 관리한다.
  - 이유: 호출 순서가 결과를 바꾸므로, 순서를 정하는 곳이 한 곳이어야 한 화면에서 흐름을 따라갈 수 있다.
- DDOL에는 `Runner` 하나만 올린다. `RuntimeInitializeOnLoadMethod(BeforeSceneLoad)`에서 생성한다.
  - 이유: 씬 배치 실수로 Runner가 없거나 둘이 되는 일이 없다.

## System
- 로직은 `~System` 이름의 **static 클래스**로 만든다. **static 필드에 상태를 두지 않는다.**
  - 이유: 상태가 모두 World에 있어야 판을 다시 시작하거나 같은 입력으로 다시 돌릴 때 숨은 값이 없다.
- **`Tick`은 그 System이 읽고 쓰는 데이터만 인자로 받는다.** 시그니처가 곧 그 System의 영향 범위다. 읽기만 하는 struct는 `in`, 수정하는 struct는 `ref`로 받는다.
  - 이유: 함수 본문을 읽지 않고도 무엇을 바꾸는지 알 수 있다. `in`/`ref`로 수정 여부가 호출부에서 보인다.

```csharp
// Runner
MoveSystem.Tick(_world.enemies, _world.enemyMovedRecords);
```

- **엔티티는 모두 `~Storage`에 담고, 생성은 SpawnSystem(틱 마지막)에서만 한다.** 하나뿐인 엔티티(플레이어)도 예외가 없다. 다른 System과 Core는 생성 요청 큐(`Queue<~SpawnRequest>`)에 넣기만 한다.
  - 이유: 순회 도중 Storage가 바뀌지 않고, 새 엔티티는 항상 다음 틱부터 동작한다. 슬롯 할당이 한 곳에 모인다.
- **SpawnSystem은 슬롯을 채울 때 필드를 하나씩 대입하지 않고 `new ~Entity { … }`로 struct 전체를 대입한다.** 이전 값에서는 `generation`만 이어받는다. 할당을 피하려고 재사용하는 배열 필드는 대입 전에 꺼내 두었다가 새 struct에 다시 넣고 내용을 비운다.
  - 이유: 엔티티에 필드를 추가하고 대입을 빠뜨리면 그 칸을 쓰던 이전 엔티티의 값이 남는다. 전체를 대입하면 빠뜨린 필드는 기본값이 된다.
- 소멸은 즉시 한다(`isAlive = false`, 슬롯 반납). 이후 System은 `isAlive`로 건너뛴다.
  - 이유: 고정 배열이라 플래그만 바꾸면 되고, 같은 틱의 뒤 System이 이미 죽은 엔티티를 처리하지 않는다.
- 엔티티는 객체 참조 대신 `EntityIdentifier { index, generation }`로 가리킨다. 엔티티 저장소(`~Storage`)는 종류별 배열과 빈 슬롯 목록으로 보관하고, 슬롯을 재사용할 때 generation을 올린다.
  - 이유: 칸이 재사용되어도 이미 사라진 엔티티를 가리키는지 generation으로 가린다. 값이라서 Record에 담을 수 있다.
- System 간 호출은 직접 호출한다.
  - 이유: 공용 판정을 중복 없이 쓰고, 호출 관계가 코드에 그대로 보인다.
- 서비스 클래스, ServiceLocator를 만들지 않는다.
  - 이유: 인자로 받지 않은 의존이 생기면 시그니처가 영향 범위를 보여 주지 못한다.

## 실패 처리
- **조용히 실패하지 않는다.** 요청을 버리거나, 찾는 대상이 없거나, 예상 밖의 값을 만나 건너뛸 때(`continue`, 이른 `return`)는 `Debug.LogWarning` 또는 `Debug.LogError`로 남긴다. 정상 흐름의 건너뛰기(`isAlive`가 false인 칸 등)는 해당하지 않는다.
  - 이유: 조용히 넘기면 증상만 보이고 단서가 없다. 조용히 틀릴 수 있는 곳이 줄수록 사람이 검토할 범위가 좁아진다.
- 매 틱 반복될 수 있는 실패는 틱당 종류별로 한 번만, 개수를 모아 남긴다.
  - 이유: 실패마다 남기면 초당 수십 줄이 쌓여 오히려 원인을 찾기 어렵다.

## 데이터와 로딩
- 수치는 하드코딩하지 않고 `~Definition`(ScriptableObject)에 둔다.
  - 이유: 수치를 코드 수정 없이 에디터에서 조정한다.
- **데이터는 struct로 품는다.** Logic이 쓰는 값은 `Components/`의 `[Serializable]` struct(`~Component`)로 정의하고, `~Definition`과 `~Authoring`은 그 struct를 필드로 가진다. 같은 필드를 두 곳에 정의하지 않는다. 프리팹 등 View용 참조는 struct 밖에 둔다.
  - 이유: 같은 값을 여러 곳에 정의하면 어긋난다. struct라서 복사 한 번으로 엔티티에 담긴다.
- **로딩:** Core가 `~Definition`의 struct를 World에 대입한다. 판 시작 시 Core는 `~Authoring`마다 Transform 위치 등 씬 값을 채운 생성 요청을 큐에 넣고, 엔티티는 첫 틱의 SpawnSystem이 만든다. Core는 World에 엔티티를 직접 넣지 않는다. System은 Definition과 Authoring을 참조하지 않는다.
  - 이유: 씬에 놓인 엔티티와 게임 중 생긴 엔티티가 같은 경로로 만들어진다. Logic이 에셋에 묶이지 않는다.
- 씬에 배치한 엔티티의 종류는 `~Authoring`이 `~Definition`을 필드로 가리켜 정한다. Authoring은 Definition을 가리키기만 하고, 판 시작 시 Core가 그 Definition의 struct를 읽어 생성 요청에 담는다.
  - 이유: 같은 종류를 씬에 여러 개 놓아도 수치는 Definition 한 곳에서 고친다.
- Authoring이 가리키지 않는 Definition은 `Resources/` 아래의 Definition Catalog 에셋 하나에 모은다(경로는 `CLAUDE.md`의 프로젝트 값). Runner가 시작할 때 `Resources.Load`로 이 Catalog만 읽는다. 다른 곳에서 `Resources.Load`를 쓰지 않는다.
  - 이유: 로드 경로가 한 곳이라 에셋 누락이 Runner 시작 시점에 바로 드러난다.
- 테이블은 생성 시점에만 조회하고 필요한 값은 엔티티에 복사한다. 매 틱 로직은 엔티티 자신의 값만 쓴다.
  - 이유: 엔티티 값만 보면 동작을 알 수 있고, 진행 중에 에셋을 바꿔도 판이 흔들리지 않는다.

## 이벤트 금지
- C# `event`, delegate 구독, 이벤트 버스, 이벤트 큐를 만들지 않는다.
  - 이유: 이벤트 체인은 흐름이 코드에 보이지 않고 구독 순서, 수명, 재진입 문제를 만들어 디버깅이 가장 어렵다. 사건은 Record로 남기고 정해진 시점에 읽는다.
- **예외: UI.** `Button.onClick` 등 UnityEvent와, `UserInterface/` 안에서 UI View가 발행해 Presenter가 구독하는 이벤트는 허용한다. 단 핸들러는 게임 상태를 직접 바꾸지 않고 명령 버퍼에 명령을 넣는 것까지만 한다.
  - 이유: 버튼 입력은 본래 이벤트다. 핸들러가 명령만 넣으면 게임 상태로 가는 길은 Command 하나로 유지된다.

## View
- Logic/View 분리는 코드 차원의 약속이며 어셈블리 참조로 지킨다. View 코드는 모두 `Presentation/` 어셈블리에 둔다.
  - 이유: 규칙을 잊어도 컴파일 오류로 잡힌다.
- `Presentation/`은 영역별 하위 폴더로 나누고, 영역 폴더 안은 `ViewSystems/`(`~ViewSystem`)와 `Views/`(`~View` MonoBehaviour)로 나눈다. 여러 영역이 함께 쓰는 도구는 역할 이름의 하위 폴더에 둔다.
  - 이유: 영역 이름만 알면 그 연출 코드의 위치를 안다.

```
Presentation/
├── ViewContext.cs
├── Player/
│   ├── ViewSystems/    PlayerViewSystem
│   └── Views/          PlayerView
├── Enemy/
│   ├── ViewSystems/
│   └── Views/
├── Audio/              오디오 재생
├── Pooling/            오브젝트 풀
└── Camera/             카메라
```

- `Presentation/`의 `~View`는 인스펙터에서 연결한 참조를 public 필드로 가진다. 메서드를 두지 않는다.
  - 이유: 연출 판단이 ViewSystem 한 곳에 모이고, View는 프리팹의 참조 묶음으로만 남는다.
- 연출의 판단과 동작은 모두 `~ViewSystem`(static)이 한다. Animator 파라미터 이름 등은 `~ViewSystem`의 상수로 둔다.
  - 이유: 문자열 이름이 흩어지면 오타가 실행해 봐야 드러난다.
- View는 Runner가 LateUpdate에서 정해진 순서로 호출하는 `~ViewSystem.Present`가 처리한다. 인자는 System처럼 필요한 데이터와 ViewContext만 받는다.
  - 이유: 연출의 순서와 읽는 데이터도 Logic처럼 한 곳에서 보인다.
- View 쪽 실행 상태(EntityIdentifier → View 매핑, 풀, 카메라 참조)는 **ViewContext 인스턴스**에 둔다. ViewSystem도 static 필드에 상태를 두지 않는다.
  - 이유: 판을 다시 시작해도 이전 판의 View 매핑이 남지 않는다.
- 지속 상태는 매 프레임 엔티티 저장소를 읽고, 그 순간의 일은 프레임 기록을 읽는다. 프레임 기록은 struct이며 객체 참조를 담지 않는다. 기록 목록은 Runner가 LateUpdate 마지막에 비운다.
  - 이유: 지속 상태는 놓친 프레임이 있어도 따라잡고, 사건은 한 번만 재생된다. 객체 참조가 없으니 이미 사라진 객체를 가리키지 않는다.
- View 오브젝트는 살아 있는 엔티티와 비교해 생성·반납한다. 게임 루프에서 `Instantiate` / `Destroy`를 반복하지 않고 풀을 쓴다.
  - 이유: 게임 중 할당과 GC 스파이크를 막는다.
- UI는 MVP 구조다. `UserInterface/`은 화면 영역별 하위 폴더로 나누고, 영역 폴더 안은 `Views/`(`~View`)와 `Presenters/`(`~Presenter`)로 나눈다.
  - 이유: 화면 영역 이름만 알면 그 UI 코드의 위치를 안다.
- UI View는 버튼 입력을 이벤트로 발행하고 표시 메서드(`SetHealth` 등)를 가진다. Presenter는 MonoBehaviour이며, View의 이벤트를 구독해 명령 버퍼에 넣고, Runner에게 필요한 데이터만 받아 View의 표시 메서드를 부른다. Model 변경 알림은 없고 Runner가 매 프레임 데이터를 넘긴다.
  - 이유: 게임 데이터를 아는 쪽은 Presenter뿐이라 화면을 바꿔도 로직이 안전하다. 매 프레임 넘기므로 변경 알림 이벤트가 필요 없다.

## UI 계층 (uGUI)
- 모든 UI는 같은 계층을 가진다.
  - Root(이름 붙임): View 스크립트가 붙는다. width/height를 가진다.
    - Content: View가 켜고 끄는 오브젝트. Vertical Layout Group이 붙는다.
      - Header / Body / Footer: 셋 다 항상 있다. 각각 Layout Element로 자기 높이를 정한다.
  - 이유: HTML처럼 구조가 하나라서, 어느 UI든 같은 위치에서 같은 역할의 오브젝트를 찾는다.
- Canvas 밑에 Panel을 두고 그 아래에 UI Root들을 모은다. HUD도 요소(체력, 탄약, 낮밤 등)마다 Root와 View를 따로 둔다.
  - 이유: 요소마다 따로 켜고 끄고 옮길 수 있다.
- View와 Presenter는 일대일이다. View 안에 View를 넣지 않는다.
  - 이유: Presenter 하나가 View 하나만 알면 데이터가 어디로 가는지 한 줄로 따라간다.
- 레이아웃 그룹 바로 아래 자식에는 위치 트윈을 걸지 않고, 더 안쪽 자식에 건다.
  - 이유: 레이아웃 그룹이 바로 아래 자식의 위치를 덮어써서 트윈이 무시되거나 튄다.

## 입력
- 프레임 단위 입력(`wasPressedThisFrame` 등)은 Update에서만 읽는다.
  - 이유: FixedUpdate에서 읽으면 한 프레임에 틱이 0번이나 2번 돌 때 입력을 놓치거나 두 번 읽는다.
- Runner가 Update에서 명령 버퍼에 쌓고, CommandSystem이 Tick에서 소비한다.
  - 이유: Logic은 입력이 어디서 왔는지 모르고 명령만 본다.

## 단위
- 틱 길이는 Project Settings > Time > Fixed Timestep이 정한다(값은 `CLAUDE.md`의 프로젝트 값). 코드에 틱 길이 상수를 두지 않고, 필요하면 `Time.fixedDeltaTime`을 읽는다. 시간 값은 **정수 틱**으로 다루고 필드명은 `~Ticks`로 끝낸다.
  - 이유: 값이 설정과 코드 두 곳에 있으면 어긋난다. 정수 틱은 float 누적 오차 없이 정확히 맞아떨어진다.
- 거리는 월드 유닛(1 = 1m), XZ 평면 기준. 거리 비교는 `sqrMagnitude`로 한다.
  - 이유: 단위가 하나여야 수치를 비교할 수 있고, 비교에는 제곱근이 필요 없다.
- 랜덤은 World가 가진 시드 고정 `System.Random`을 인자로 받아 쓴다. System에서 `UnityEngine.Random`을 쓰지 않는다.
  - 이유: 같은 입력이면 같은 결과가 나와 버그를 재현할 수 있다.
- 매 틱 할당(LINQ, 클로저, 박싱, 문자열 결합)을 피한다.
  - 이유: 초당 50번 쌓이는 할당은 GC 스파이크로 프레임을 튀게 한다.

## 폴더와 어셈블리
스크립트 루트(`CLAUDE.md`의 프로젝트 값) 아래에 폴더를 둔다. 같은 종류의 타입을 모은 폴더는 복수형, 역할을 나타내는 폴더는 단수형으로 짓는다. 어셈블리 이름은 프로젝트 접두사로 시작한다. 아래 표의 `<접두사>`가 그 자리다.

| 폴더 | 내용 | 어셈블리 | 참조 가능 |
|---|---|---|---|
| `Entities/` | `~Entity` struct, `EntityIdentifier`, `~Storage` | `<접두사>.Entity` (`.asmdef`) | 없음 |
| `Components/` | `~Component` struct | `<접두사>.Entity` (`.asmref`) | 〃 |
| `Records/` | `~Record` struct | `<접두사>.Entity` (`.asmref`) | 〃 |
| `Requests/` | `~Request` struct | `<접두사>.Entity` (`.asmref`) | 〃 |
| `Commands/` | `~Command` struct, `CommandBuffer` | `<접두사>.Entity` (`.asmref`) | 〃 |
| `Systems/` | static 로직 | `<접두사>.Systems` | Entity |
| `Definitions/` | `~Definition` (ScriptableObject 데이터) | `<접두사>.Definitions` | Entity |
| `Authoring/` | `~Authoring` (씬 배치 MonoBehaviour) | `<접두사>.Authoring` | Entity, Definitions |
| `Persistence/` (필요할 때만) | 설정·기록 저장과 불러오기 | `<접두사>.Persistence` | Entity |
| `Presentation/` | `~ViewSystem`, `~View`, ViewContext, 오디오, 오브젝트 풀 | `<접두사>.Presentation` | Entity, Definitions, Persistence |
| `UserInterface/` | UI View, Presenter | `<접두사>.UserInterface` | Entity, Persistence |
| `Core/` | World, Runner, 부트스트랩, 로딩, 씬 전환 | `<접두사>.Core` | 전부 |
| `Editor/` | 에디터 도구 (Gizmo, PropertyDrawer, Custom Editor) | `<접두사>.Editor` (플랫폼: Editor 전용) | 전부 |

- 같은 종류의 타입은 한 폴더에, 역할은 이름 접미사로 드러낸다.
  - 이유: 이름만 보고 그 타입이 어느 어셈블리에 있고 어떤 규칙을 따르는지 안다.
- Presentation은 게임 결과를 바꾸지 않으므로 읽을 대상(Definition의 프리팹·연출 값, 저장된 설정)을 넓게 허용한다. 단 Systems는 참조하지 않는다.
  - 이유: Systems를 참조하면 View가 `Tick`을 부를 수 있게 되어 상태를 바꾸는 길이 생긴다.
- "참조 가능"은 프로젝트 어셈블리 사이의 방향만 적는다. 외부 패키지(Input System, Cinemachine, UGUI 등) 참조는 `CLAUDE.md`의 프로젝트 값에 적는다.
  - 이유: 원칙은 프로젝트를 옮겨도 그대로 쓰고, 프로젝트마다 다른 값은 한 곳에서 고친다.
- 표에 없는 종류의 타입을 만들어야 하면, 어디에 둘지 먼저 묻는다.
  - 이유: 구조가 임의로 늘어나면 위 규칙들이 적용되지 않는 구석이 생긴다.
