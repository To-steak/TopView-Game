---
paths:
  - "Project/Assets/Scripts/**/*.cs"
  - "Project/Assets/Scripts/**/*.asmdef"
  - "Project/Assets/Scripts/**/*.asmref"
---

# 아키텍처 규칙

패키지(DOTS)는 쓰지 않고 순수 C#으로 쓴다. 엔티티는 컴포넌트를 필드로 가진 struct이고, 모든 종류가 저장소 하나의 고정 배열에 담긴다. 칸이 가진 컴포넌트는 마스크로 표시한다. System은 컴포넌트와 Request를 받아 처리하는 함수다.

규칙마다 `이유:` 줄은 그 규칙이 없으면 무엇이 깨지는지를 적는다. 이유가 성립하지 않는 상황이면 규칙을 어기기 전에 먼저 묻는다.

도구가 생성한 코드(Input System의 액션 클래스 등)는 이 규칙을 적용하지 않는다. 생성 코드는 고치지 않는다.

## 전체 흐름

```
입력 → Command → System(FixedUpdate) → Entity 변경 → Record
                                                       ↓
              View ← ViewSystem / Presenter(LateUpdate) ← Entity, Record
```

Unity의 FixedUpdate / Update / LateUpdate는 App만 받고, App이 지금 씬의 대표(World 또는 `Menu`)를 정해진 순서로 부른다. World는 자기 데이터를 꺼내 호출 순서대로 넘긴다. 게임 상태를 바꾸는 곳은 System뿐이다.

## 공통 원칙

### Logic / View 분리
- **Logic의 루프는 FixedUpdate에서만 돈다.** 게임 상태를 바꾸는 코드는 System의 `Tick` 안에만 둔다.
  - 이유: 프레임레이트에 따라 판정과 결과가 달라지지 않는다.
- **View의 루프는 Update / LateUpdate에서 돈다.** 애니메이션, SFX, VFX, UI, 카메라가 View다.
  - 이유: 화면은 프레임마다 그리되, 그리는 일이 게임 결과를 바꾸지 않는다.
- 이 구분은 매 틱·매 프레임 반복되는 일에만 적용한다. 한 번만 하는 초기화(Awake / Start)는 어디에 둬도 된다.
  - 이유: 초기화는 반복되지 않아 프레임레이트나 틱 횟수에 따라 결과가 달라지지 않는다.
- Logic은 View를 참조하지 않는다. 그 틱에 일어난 일은 프레임 기록 목록(`List<~Record>`)에 남긴다.
  - 이유: View 없이도 Logic이 돈다. 사건을 데이터로 남기면 누가 언제 읽을지가 World의 호출 순서에 고정된다.
- View는 게임 상태를 읽기만 한다. 입력은 명령 버퍼(`~Command`)를 거친다.
  - 이유: 상태를 바꾸는 곳이 System뿐이라, 상태가 틀렸을 때 System만 보면 된다.

### 이벤트 금지
- C# `event`, delegate 구독, 이벤트 버스, 이벤트 큐를 만들지 않는다.
  - 이유: 이벤트 체인은 흐름이 코드에 보이지 않고 구독 순서, 수명, 재진입 문제를 만들어 디버깅이 가장 어렵다. 사건은 Record로 남기고 정해진 시점에 읽는다.
- **예외: UI.** `Button.onClick` 등 UnityEvent와, `UserInterface/` 안에서 UI View가 발행해 Presenter가 구독하는 이벤트는 허용한다. 단 핸들러는 게임 상태를 직접 바꾸지 않고 명령 버퍼(앱 수명의 일은 App의 명령 목록)에 명령을 넣는 것까지만 한다.
  - 이유: 버튼 입력은 본래 이벤트다. 핸들러가 명령만 넣으면 게임 상태로 가는 길은 Command 하나로 유지된다.

### 실패 처리
- **조용히 실패하지 않는다.** 요청을 버리거나, 찾는 대상이 없거나, 예상 밖의 값을 만나 건너뛸 때(`continue`, 이른 `return`)는 `Debug.LogWarning` 또는 `Debug.LogError`로 남긴다. 정상 흐름의 건너뛰기(빈 칸, 이미 죽은 대상에 대한 피해·상태 변경 등)는 해당하지 않는다.
  - 이유: 조용히 넘기면 증상만 보이고 단서가 없다. 조용히 틀릴 수 있는 곳이 줄수록 사람이 검토할 범위가 좁아진다.
- 매 틱 반복될 수 있는 실패는 틱당 종류별로 한 번만, 개수를 모아 남긴다.
  - 이유: 실패마다 남기면 초당 수십 줄이 쌓여 오히려 원인을 찾기 어렵다.

### 단위와 할당
- 틱 길이는 Project Settings > Time > Fixed Timestep이 정한다(값은 `CLAUDE.md`의 프로젝트 값). 코드에 틱 길이 상수를 두지 않고, 필요하면 `Time.fixedDeltaTime`을 읽는다. 시간 값은 **정수 틱**으로 다루고 필드명은 `~Ticks`로 끝낸다.
  - 이유: 값이 설정과 코드 두 곳에 있으면 어긋난다. 정수 틱은 float 누적 오차 없이 정확히 맞아떨어진다.
- 거리는 월드 유닛(1 = 1m), XZ 평면 기준. 거리 비교는 `sqrMagnitude`로 한다.
  - 이유: 단위가 하나여야 수치를 비교할 수 있고, 비교에는 제곱근이 필요 없다.
- 매 틱 할당(LINQ, 클로저, 박싱, 문자열 결합)을 피한다.
  - 이유: 초당 50번 쌓이는 할당은 GC 스파이크로 프레임을 튀게 한다.

## 폴더 한눈에 보기

스크립트 루트(`CLAUDE.md`의 프로젝트 값) 아래에 폴더를 둔다. 같은 종류의 타입을 모은 폴더는 복수형, 역할을 나타내는 폴더는 단수형으로 짓는다. 어셈블리 이름은 프로젝트 접두사로 시작한다. 아래 표의 `<접두사>`가 그 자리다.

| 폴더 | 존재 이유 | 내용 | 어셈블리 | 참조 가능 |
|---|---|---|---|---|
| `Entities/` | 게임 속 대상과 저장 칸 | `Entity`, `EntityStorage`, `ComponentMask`, `EntityArchetype`, `EntityIdentifier`, 행동 상태 enum(`~State`) | `<접두사>.Entity` (`.asmdef`) | 없음 |
| `Components/` | 모든 데이터의 묶음 | `~Component` struct | `<접두사>.Entity` (`.asmref`) | 〃 |
| `Requests/` | 틱 안에서 System에 일을 맡김 | `~Request` struct | `<접두사>.Entity` (`.asmref`) | 〃 |
| `Records/` | 일어난 일을 View에 전달 | `~Record` struct | `<접두사>.Entity` (`.asmref`) | 〃 |
| `Commands/` | 입력을 Logic에 전달 | `~Command` struct, `CommandBuffer` | `<접두사>.Entity` (`.asmref`) | 〃 |
| `Systems/` | 게임 상태 변경 | `~System` (static 로직) | `<접두사>.Systems` | Entity |
| `Utilities/` | 값 계산 도구 | `~Utility` (static 도구) | `<접두사>.Systems` (`.asmref`) | 〃 |
| `Definitions/` | 에디터에서 조정하는 수치 | `~Definition` (ScriptableObject 데이터) | `<접두사>.Definitions` | Entity |
| `Authoring/` | 씬 배치 정보 | `~Authoring` (씬 배치 MonoBehaviour) | `<접두사>.Authoring` | Entity, Definitions |
| `Persistence/` (필요할 때만) | 설정, 기록 저장 | `~Store` (설정·기록 저장과 불러오기) | `<접두사>.Persistence` | Entity |
| `Presentation/` | 게임 세계의 연출 | `~ViewSystem`, `~View`, `ViewContext`, `~ViewContext`, 오디오, 오브젝트 풀 | `<접두사>.Presentation` | Entity, Definitions, Persistence |
| `UserInterface/` | 화면 UI | UI View, View가 소유하는 부품(`~Entry`), Presenter | `<접두사>.UserInterface` | Entity, Persistence |
| `Core/` | 데이터 보관과 실행 순서 | App(앱 수명의 일, 부트스트랩, 로딩, 씬 전환), 씬의 대표(World, `Menu`) | `<접두사>.Core` | 전부 |
| `Editor/` | 에디터 도구 | 에디터 도구 (Gizmo, PropertyDrawer, Custom Editor) | `<접두사>.Editor` (플랫폼: Editor 전용) | 전부 |
| `Tests/` | 동작 확인 | `~Tests` (EditMode 테스트) | `<접두사>.Tests` (플랫폼: Editor 전용) | Entity, Systems |
| `PlayModeTests/` | 씬과 틱 순서, 표시의 동작 확인 | `~Tests` (PlayMode 테스트) | `<접두사>.PlayModeTests` | Entity, Systems, Core, Presentation, UserInterface |

- 같은 종류의 타입은 한 폴더에, 역할은 이름 접미사로 드러낸다.
  - 이유: 이름만 보고 그 타입이 어느 어셈블리에 있고 어떤 규칙을 따르는지 안다.
- "참조 가능"은 프로젝트 어셈블리 사이의 방향만 적는다. 외부 패키지(Input System, Cinemachine, UGUI 등) 참조는 `CLAUDE.md`의 프로젝트 값에 적는다.
  - 이유: 원칙은 프로젝트를 옮겨도 그대로 쓰고, 프로젝트마다 다른 값은 한 곳에서 고친다.
- 표에 없는 종류의 타입을 만들어야 하면, 어디에 둘지 먼저 묻는다.
  - 이유: 구조가 임의로 늘어나면 위 규칙들이 적용되지 않는 구석이 생긴다.

## 폴더별 설명

### Entities/
게임 속 대상 하나를 고정 배열의 칸으로 표현한다.

- **엔티티는 저장소 하나(`EntityStorage`)의 고정 배열 칸(`Entity`)이다.** `Entity`는 모든 컴포넌트를 필드로 가진 struct이고, 칸마다 가진 컴포넌트를 표시하는 `mask`(`ComponentMask`)와 `generation`을 둔다. `mask`가 `None`이면 빈 칸이다. 하나뿐인 엔티티(플레이어)도 예외가 없다. 게임 중 바뀌는 값도 컴포넌트로 묶어 필드로 둔다.
  - 이유: 종류별 저장소로 나누면 종류마다 다른 컴포넌트 조합(근접·원거리·투척 무기 등)을 다룰 때 "지금 대상이 어느 저장소에 있나"를 읽는 곳마다 갈라야 한다. 칸 모양이 하나면 식별자 하나로 찾아가고, 컴포넌트가 있는지만 보면 된다.
- **System은 처리할 컴포넌트의 비트가 모두 켜진 칸만 돈다.** 마스크에는 모든 컴포넌트가 아니라 엔티티마다 있고 없음이 갈리는 것만 비트로 둔다. 켜지지 않은 컴포넌트 필드의 값은 뜻이 없다.
  - 이유: 꺼진 칸의 값은 이전 엔티티의 값이거나 기본값이라, 읽으면 다른 엔티티의 값으로 동작한다.
- **엔티티 종류별 컴포넌트 조합은 `EntityArchetype`의 상수로 모은다.** 칸을 채우는 `~SpawnSystem`과 Core는 이 값을 마스크로 쓴다. 처리하는 System은 이 값을 쓰지 않고 자기가 읽는 컴포넌트만 마스크로 고른다.
  - 이유: 종류별 타입이 없어 조합이 생성 코드마다 흩어지면 프로젝트에 어떤 엔티티가 있는지 한곳에서 볼 수 없다. 처리하는 System이 종류 조합을 쓰면 컴포넌트가 같은 다른 종류를 놓친다.
- **보스처럼 특별한 적을 별도 종류로 만들지 않는다.** 차이는 적의 값으로 표현한다. 예: `isGameOverOnDeath`, `hasOverlayHealthBar`
  - 이유: 종류가 늘면 피해, 피격, View마다 반복이 생긴다.
- **다른 엔티티가 들고 쓰는 것이라도 자기만의 변하는 상태가 있으면 엔티티로 둔다.** 예: 무기(탄약, 강화 레벨). 주인 쪽에 번호로 줄을 맞춘 배열(`currentAmmo[무기 번호]` 등)로 그 상태를 두지 않는다.
  - 이유: 줄을 맞춘 배열은 "같은 번호가 같은 대상"이라는 약속으로만 묶여 있어, 한쪽 배열만 고치면 어긋나도 드러나지 않는다. 상태가 대상 곁에 있어야 그 대상만 받는 함수를 쓸 수 있다.
- **들린 엔티티와 주인은 서로를 식별자로 가리킨다.** 들린 쪽은 `OwnerComponent`로 주인을, 주인은 든 것의 식별자 목록을 가진다. 주인의 목록은 들린 쪽의 `~SpawnSystem`이 채운다.
  - 이유: 주인에서 든 것을 찾을 때 저장소를 훑지 않고, 든 것에서 주인이 사라졌는지 확인할 수 있다. 채우는 곳이 하나라 양쪽이 어긋나지 않는다.
- **주인이 사라진 엔티티는 `~OwnerDeathSystem`이 소멸을 요청한다.** 주인의 칸이 비었거나 generation이 달라졌을 때 요청하고, 주인이 사망 상태에 들어간 것만으로는 요청하지 않는다(사망 연출 동안 View가 읽는다). 사망 상태에 맞춰 없애야 하는 엔티티(근접 판정)는 그 조건을 따로 적는다.
  - 이유: 주인 없는 엔티티가 칸을 차지한 채 남으면, 칸이 재사용된 뒤 새 주인의 것으로 착각한다. 소멸을 요청 큐로만 하면 칸을 비우는 코드가 `~DespawnSystem` 한 곳에 모인다.
- 엔티티는 객체 참조 대신 `EntityIdentifier { index, generation }`로 가리킨다. 칸을 재사용할 때 generation을 올린다. 모든 종류가 같은 번호 공간을 쓴다.
  - 이유: 칸이 재사용되어도 사라진 엔티티를 가려낸다.
- 예외: 판을 준비할 때 Core가 잇는 씬 배치 엔티티끼리의 연결(감지기 → 스포너)은 식별자 대신 Core가 붙인 연결 번호로 가리킨다.
  - 이유: 식별자는 `~SpawnSystem`이 칸을 채운 뒤에 정해지고, 이 엔티티들은 판 중에 사라지지 않아 칸이 다시 쓰이지 않는다.
- 저장소의 칸은 엔티티 종류별 구간(적은 적 종류별)으로 나누고, 구간마다 빈 칸 목록을 둔다.
  - 이유: 한 종류가 많이 생겨도 다른 종류의 칸을 차지하지 않는다.

### Components/
모든 데이터를 struct로 묶어 한 곳에 정의한다.

- **데이터는 모두 `Components/`의 struct(`~Component`)로 품는다.** 에디터에서 정하는 설정값은 `[Serializable]`을 붙이고 `~Definition`과 `~Authoring`이 그 struct를 필드로 가진다. 게임 중 바뀌는 런타임 값은 `[Serializable]`을 붙이지 않고 주석에 런타임 값임을 밝힌다. 같은 필드를 두 곳에 정의하지 않는다. 프리팹 등 View용 참조는 struct 밖에 둔다.
  - 이유: 같은 값을 여러 곳에 정의하면 어긋난다. struct라서 복사 한 번으로 칸에 담긴다.
- **`Components/`는 개념별 하위 폴더로 나눈다.** 소유자(`Player/`, `Enemy/`) 기준으로 나누지 않는다. 한 컴포넌트가 두 폴더에 걸치면 개념이 섞인 것이니 쪼갤지 먼저 묻는다.
  - 이유: 뜻이 같은 데이터가 한 폴더에 모여야 겹치는 필드가 보인다. 소유자로 나누면 같은 데이터가 폴더마다 따로 생긴다.

```
Components/
├── Action/      행동 상태와 행동 시간
├── Attack/      공격을 내보내는 방식(판정 방식, 발, 시간), 진영, 피격 대상, 적 공격 목록
├── Game/        판과 메뉴의 규칙, 진행, 설정
├── Loot/        전리품
├── Melee/       근접 판정
├── Movement/    위치, 회전, 이동
├── Projectile/  투사체의 비행, 판정, 폭발, View
├── Spawning/    스포너, 감지기
├── Upgrade/     강화
├── Vitals/      체력, 마나, 능력치
└── Weapon/      들고 쓰는 무기, 주인, 탄약, 투척 조준
```

- 하위 폴더의 네임스페이스는 폴더 경로를 따르지 않고 모두 `<접두사>.Components`로 둔다.
  - 이유: 하위 폴더는 분류일 뿐이라 컴포넌트를 다른 개념 폴더로 옮기거나 합쳐도 쓰는 쪽 코드가 바뀌지 않는다.
- 설정값 컴포넌트는 생성 때 복사한 뒤 게임 중에 바꾸지 않는다. 시작값이 필요한 런타임 값은 설정값 컴포넌트에 `starting~` 필드로 두고, 생성 때 런타임 컴포넌트로 옮긴다.
  - 이유: 값이 바뀌는 곳이 런타임 컴포넌트뿐이어야 타입만 보고 설정값인지 상태인지 안다.
- **진영(`Faction`)은 공격 쪽(`AttackPayloadComponent`)에만 둔다.** 어느 쪽(플레이어·적)의 피격 대상 목록을 볼지는 공격의 진영으로 정한다. 피격 대상 목록과 피해 요청은 방향별로 나누고, 체력을 깎는 함수는 공유한다. 플레이어와 적은 그쪽에만 있는 컴포넌트(`PlayerActionState`, `EnemyKind`)의 비트로 가린다. `EntityKind` 같은 종류 값이나 방향 bool을 만들지 않는다.
  - 이유: 대상 쪽에 진영이나 종류를 두면 값 하나를 고칠 때 비교하는 곳이 모두 따라와야 한다.

### Requests/
다른 System에게 틱 안에서 처리할 일을 맡긴다.

- **생성은 엔티티 종류별 `~SpawnSystem`(틱 마지막)에서만 한다.** 다른 System과 Core는 생성 요청 큐(`Queue<~SpawnRequest>`)에 넣기만 한다.
  - 이유: 순회 도중 칸이 채워지지 않고, 새 엔티티는 항상 다음 틱부터 동작한다.
- **소멸은 요청 큐(`Queue<~DespawnRequest>`)에 넣고, `~DespawnSystem`만 처리한다**(`mask`를 `None`으로 바꾸고 칸 번호를 그 구간의 빈 칸 목록에 돌려준다). World는 요청을 넣는 System 바로 뒤에 `~DespawnSystem`을 호출한다. 이후 System은 마스크로 건너뛴다.
  - 이유: 칸을 비우는 코드가 한 곳에 모이고, 같은 틱의 뒤 System이 이미 소멸한 엔티티를 처리하지 않는다.

### Records/
그 틱에 일어난 일을 View가 읽도록 남긴다.

- 프레임 기록은 struct이며 객체 참조를 담지 않는다. 기록 목록은 World가 `Present` 마지막(App이 LateUpdate에서 부른다)에 비운다.
  - 이유: 사건은 한 번만 재생된다. 객체 참조가 없으니 이미 사라진 객체를 가리키지 않는다.

### Commands/
입력을 Logic에 전달한다.

- 프레임 단위 입력(`wasPressedThisFrame` 등)은 Update에서만 읽는다.
  - 이유: FixedUpdate에서 읽으면 한 프레임에 틱이 0번이나 2번 돌 때 입력을 놓치거나 두 번 읽는다.
- World가 매 프레임(App이 Update에서 부른다) 명령 버퍼에 쌓고, CommandSystem이 Tick에서 소비한다.
  - 이유: Logic은 입력이 어디서 왔는지 모르고 명령만 본다.
- **명령 목록(`~Command` 타입) 하나를 소비하는 곳은 하나다.** 소비하는 곳이 다른 명령은 같은 목록에 넣지 않고 `~Command` 타입을 나눈다. 명령 이름은 소비하는 곳의 일을 드러낸다.
  - 이유: 여러 곳이 한 목록을 나눠 소비하면 각자 남의 명령을 건너뛰고 남겨 두는 코드가 생기고, 종류를 하나 더할 때 상관없는 소비처까지 고쳐야 한다.
- 순서대로 하나씩 처리할 요청은 목록(`List<~Command>`)에 쌓는다. 지금의 입력 상태만 알면 되는 입력(이동 방향, 누르고 있는 키, 조준점)은 목록 없이 `~Command` struct 한 칸에 둔다. World가 Update마다 그 칸을 덮어쓰고, 한 번 누르는 입력은 bool 표시로 두어 틱이 소비할 때까지 남긴다.
  - 이유: 상태형 입력을 목록에 쌓으면 틱 사이 프레임 수만큼 쌓여도 쓰는 값은 마지막 하나다. 같은 틱에 두 번 누른 입력은 한 번으로 합쳐져야 해서, 목록이면 겹친 명령을 어떻게 다룰지 따로 정해야 한다.
- struct 한 칸의 입력은 여러 System이 `in`으로 읽어도 되고, 한 번 누르는 입력의 표시를 지우는 곳은 CommandSystem 하나다.
  - 이유: 꺼내 가는 방식이면 먼저 읽은 System이 뒤의 System에게서 입력을 빼앗는다. 지우는 곳이 여럿이면 같은 틱 안에서 System마다 보는 입력이 달라진다.
- 앱 수명의 명령(씬 전환과 종료, 설정 화면)은 App이 명령 목록(`List<~Command>`)을 가지고 Update에서 소비한다. 어느 씬의 Presenter가 넣었든 같은 목록에 넣는다.
  - 이유: 씬마다 같은 명령을 따로 소비하면 같은 처리 코드가 씬 수만큼 생긴다. 앱 수명의 상태를 가진 App 한 곳에서 소비하면 된다.
- 테스트용 디버그 명령(`DebugCommand`)은 World의 에디터 전용 Context Menu(`World.Debug.cs`, 파일 전체를 `#if UNITY_EDITOR`로 감쌈)가 Update 밖에서 넣고, `DebugCommandSystem`이 다음 틱에 소비한다. 넣는 양 같은 디버그 값은 Definition이 아니라 `World.Debug.cs`의 상수로 둔다.
  - 이유: 디버그 명령은 한 번 넣으면 소비될 때까지 목록에 남아 프레임과 틱이 어긋나도 놓치거나 두 번 읽지 않는다. 상태는 여전히 System만 바꾸고, 디버그 값이 Definition에 섞이면 게임 수치로 오해한다.

### Systems/
게임 상태를 바꾸는 유일한 곳이다.

- 로직은 `~System` 이름의 **static 클래스**로 만든다. **static 필드에 상태를 두지 않는다.**
  - 이유: 상태가 모두 World에 있어야 판을 다시 시작하거나 같은 입력으로 다시 돌릴 때 숨은 값이 없다.
- **`Systems/`는 `Components/`의 개념 폴더 이름에 맞춘 하위 폴더로 나눈다.** 소유자(`Player/`, `Enemy/`) 기준으로 나누지 않는다. 생성·소멸 System은 그 엔티티의 개념 폴더에 두고, 플레이어와 적 엔티티의 생성·소멸은 `Spawning/`에 둔다.
  - 이유: 플레이어와 적이 하는 같은 일(상태 시간, 사망, 이동)이 한 폴더에 모여야 합칠 수 있는 System이 보인다.

```
Systems/
├── Game/        명령 소비, 판 규칙, 일시정지
├── Action/      행동 상태의 결정, 변경, 시간, 사망
├── Movement/    직전 위치 기록, 이동, 조준, 길 찾기, 플레이어 위치 기록
├── Attack/      무기로 공격 내보내기, 피격 대상 목록
├── Melee/       근접 판정의 생성, 명중, 소멸
├── Projectile/  투사체의 생성, 비행, 명중, 소멸
├── Weapon/      무기의 생성, 소멸, 투척 미리보기
├── Vitals/      피해, 마나, 적 레벨
├── Loot/        전리품의 생성, 수명, 획득, 소멸
├── Spawning/    플레이어와 적의 생성·소멸, 스포너, 감지기
└── Upgrade/     강화
```

- 하위 폴더의 네임스페이스는 폴더 경로를 따르지 않고 모두 `<접두사>.Systems`로 둔다.
  - 이유: `Components/`와 같다. 하위 폴더는 분류일 뿐이라 옮겨도 World의 호출 코드가 바뀌지 않는다.
- **`Tick`은 그 System이 쓰는 저장소, 요청 큐, 기록 목록만 인자로 받는다.** 엔티티는 `EntityStorage` 하나로 받고, 처리하는 칸의 컴포넌트(마스크)와 읽고 쓰는 컴포넌트는 문서 주석의 저장소 인자 설명에 적는다. 읽기만 하는 struct는 `in`, 수정하는 struct는 `ref`로 받는다.
  - 이유: 함수 본문을 읽지 않고도 무엇을 바꾸는지 알 수 있다. 컴포넌트 배열을 하나씩 받으면 인자가 수십 개로 늘어 오히려 읽기 어렵다. `in`/`ref`로 수정 여부가 호출부에서 보인다.

```csharp
// LootLifetimeSystem
/// <param name="storage">엔티티 저장소. Loot를 가진 칸만 처리하고, loot의 남은 수명을 줄인다</param>
/// <param name="despawnRequests">전리품 소멸 요청 큐</param>
public static void Tick(EntityStorage storage, Queue<LootDespawnRequest> despawnRequests)
```

- **실제 로직은 가능한 한 컴포넌트와 Request만 받는 함수에 둔다.** `Tick`은 대상을 찾아 그 함수를 부르는 얇은 반복문으로 둔다. 예: `Apply(ref HealthComponent health, in DamageRequest request)`
  - 이유: 함수 시그니처만 보고 무엇을 바꾸는지 알고, 저장소 구조를 몰라도 로직을 읽을 수 있다.
- **System은 다른 System을 호출하지 않는다.** 저장소, 요청 큐, 기록 목록을 다루는 코드는 System에 두고, 값과 컴포넌트만 다루는 함수는 `~Utility`에 둔다.
  - 이유: 호출 안에 다른 System의 변경이 숨으면 `Tick` 시그니처와 World의 호출 순서만 보고 언제 무엇이 바뀌는지 알 수 없다. 저장소와 큐를 받지 않는 함수는 시그니처만 보고 영향 범위를 안다.
- **칸을 채울 때는 `new Entity { mask = …, … }`로 칸 전체를 대입한다.** `generation`은 1 올린다. 할당을 피하려고 재사용하는 배열은 먼저 꺼내 비운 뒤 새 값에 넣는다.
  - 이유: 필드를 빠뜨려도 이전 엔티티의 값이 남지 않고 기본값이 된다. 마스크를 함께 정해야 켜진 컴포넌트와 채운 값이 어긋나지 않는다.
- **Transform은 View만 쓴다.** Logic은 위치를 데이터(`PositionComponent`의 `previous`, `current`)로 소유하고, View가 보간해 반영한다. World가 명령을 만들 때 View의 Transform을 읽는 것(카메라 방향, 총구 위치 등)은 허용한다.
  - 이유: 위치의 정본이 둘이면 판정 위치와 보이는 위치가 어긋난다.
- 판정 시점은 Logic의 타이머가 정한다. 애니메이션 이벤트로 게임 상태를 바꾸지 않는다.
  - 이유: 애니메이션 이벤트는 블렌딩, 컬링, 프레임레이트에 따라 늦거나 빠지거나 두 번 불린다.
- Unity 물리는 질의로만 쓴다. 지형 등 씬에 배치한 정적 오브젝트에 Collider를 붙이고, System이 `Tick` 안에서 직접 또는 Utility를 거쳐 `Physics` 질의(Raycast, SphereCast, Overlap 등)를 호출해 그 결과로 위치 데이터를 고친다. Utility는 질의 결과를 돌려주기만 하고 데이터를 고치지 않는다.
  - 이유: 질의는 호출 시점과 결과가 System 코드 안에서 보인다. 여러 System이 같은 질의를 써야 할 때 Utility 하나에 두면 결과가 어긋나지 않는다.
- NavMesh도 질의로만 쓴다. System이 `Tick` 안에서 `NavMesh` 질의(CalculatePath, SamplePosition 등)를 호출해 경로를 얻고, 위치 데이터는 System이 직접 옮긴다. NavMeshAgent는 쓰지 않는다.
  - 이유: NavMeshAgent는 자체 업데이트에서 Transform을 옮기고 회피를 계산해, Tick 밖의 시점에 위치가 바뀐다.
- Rigidbody, 물리 시뮬레이션, 충돌 콜백(`OnCollision~`, `OnTrigger~`)은 쓰지 않는다. 엔티티는 Collider를 갖지 않으며, 엔티티끼리의 충돌과 범위 판정은 Logic이 직접 계산한다.
  - 이유: 물리 시뮬레이션은 Tick 밖의 시점에 위치를 바꾸고, 충돌 콜백은 이벤트처럼 호출 순서가 숨는다.
- 랜덤은 World가 가진 시드 고정 `System.Random`을 인자로 받아 쓴다. System에서 `UnityEngine.Random`을 쓰지 않는다.
  - 이유: 같은 입력이면 같은 결과가 나와 버그를 재현할 수 있다.

### Utilities/
저장소와 큐 없이 값과 컴포넌트만 다루는 함수를 모은다.

- `~Utility`는 **static 클래스**다. `Tick`을 두지 않고, static 필드에 상태를 두지 않으며, System을 호출하지 않는다.
  - 이유: 틱 흐름은 System과 World만 정하고, Utility는 인자로 받은 값과 컴포넌트 밖에 영향을 주지 않는다.

### Definitions/
수치를 코드 수정 없이 에디터에서 조정한다.

- 수치는 하드코딩하지 않고 `~Definition`(ScriptableObject)에 둔다.
  - 이유: 수치를 코드 수정 없이 에디터에서 조정한다.
- 예외: 저장소 칸 수처럼 게임 조정 값이 아닌 크기는 Core의 상수로 둔다.
  - 이유: 메모리 크기는 플레이하며 조정하는 값이 아니고, 인스펙터에 두면 게임 수치처럼 보여 잘못 건드린다.
- Definition은 씬에 배치한 오브젝트가 인스펙터 필드로 가리킨다. 엔티티 종류는 `~Authoring`이, 판 전체의 Definition은 World가, 판이 없는 씬의 Definition은 그 씬의 대표(`Menu` 등)가, 앱 수명의 Definition은 App 프리팹이 가리킨다. 씬마다 다른 값(배경음악 곡 등)은 그 씬의 대표가 가리키고 App에 등록할 때 넘긴다. `Resources.Load`는 쓰지 않는다.
  - 이유: 참조가 인스펙터에 보여 무엇을 쓰는지 씬에서 바로 알고, 누락은 판 시작 시 null 검사에서 드러난다. `Resources/`는 문자열 경로에 의존하고 안의 에셋을 모두 빌드에 넣는다.
- 테이블은 생성 시점에만 조회하고 필요한 값은 칸의 컴포넌트로 복사한다. 매 틱 로직은 칸의 컴포넌트만 쓴다.
  - 이유: 컴포넌트 값만 보면 동작을 알 수 있고, 진행 중에 에셋을 바꿔도 판이 흔들리지 않는다.

### Authoring/
씬에 놓인 엔티티의 종류와 위치를 알려 준다.

- 씬에 배치한 엔티티의 종류는 `~Authoring`이 `~Definition`을 필드로 가리켜 정한다. Authoring은 Definition을 가리키기만 하고, 판 시작 시 Core가 그 Definition의 struct를 읽어 생성 요청에 담는다.
  - 이유: 같은 종류를 씬에 여러 개 놓아도 수치는 Definition 한 곳에서 고친다.

### Persistence/
설정과 기록을 저장하고 불러온다. 필요할 때만 만든다.

- 저장과 불러오기는 저장 대상별 `~Store`(static 클래스)가 한다. 저장 키 이름은 `~Store`의 상수로 둔다. static 필드에 상태를 두지 않는다.
  - 이유: 키 이름이 흩어지면 오타로 다른 칸에 저장해도 실행해 봐야 드러난다. 저장 형식을 바꿀 때 `~Store` 하나만 고친다.

### Presentation/
게임 세계의 연출을 맡고, 게임 결과는 바꾸지 않는다.

- Logic/View 분리는 코드 차원의 약속이며 어셈블리 참조로 지킨다. View 코드는 모두 `Presentation/` 어셈블리에 둔다.
  - 이유: 규칙을 잊어도 컴파일 오류로 잡힌다.
- Presentation은 게임 결과를 바꾸지 않으므로 읽을 대상(Definition의 프리팹·연출 값, 저장된 설정)을 넓게 허용한다. 단 Systems는 참조하지 않는다.
  - 이유: Systems를 참조하면 View가 `Tick`을 부를 수 있게 되어 상태를 바꾸는 길이 생긴다.
- `Presentation/`은 타입 역할별 하위 폴더로 나눈다. `Views/`(`~View` MonoBehaviour), `ViewSystems/`(`~ViewSystem`), `ViewContexts/`(`ViewContext`, `~ViewContext`)에 둔다. 여러 영역이 함께 쓰는 도구는 역할 이름의 하위 폴더에 둔다.
  - 이유: 타입의 접미사만 알면 그 파일의 위치를 안다.

```
Presentation/
├── Views/              PlayerView, EnemyView
├── ViewSystems/        PlayerViewSystem, EnemyViewSystem
├── ViewContexts/       ViewContext, PlayerViewContext
├── Audio/              오디오 재생
└── Pooling/            오브젝트 풀
```

- `Presentation/`의 `~View`는 인스펙터에서 연결한 참조를 public 필드로 가진다. 메서드를 두지 않는다.
  - 이유: 연출 판단이 ViewSystem 한 곳에 모이고, View는 프리팹의 참조 묶음으로만 남는다.
- 연출의 판단과 동작은 모두 `~ViewSystem`(static)이 한다. Animator 파라미터 이름 등은 `~ViewSystem`의 상수로 둔다.
  - 이유: 문자열 이름이 흩어지면 오타가 실행해 봐야 드러난다.
- View는 App이 LateUpdate에서 부르는 씬의 대표(World, `Menu`)와 App 자신이 정해진 순서로 호출하는 `~ViewSystem.Present`가 처리한다. 인자는 System처럼 필요한 데이터와 자기 영역의 `~ViewContext`만 받는다. 저장소 대신 읽기만 하는 엔티티 배열(`Entity[]`)을 받는다. 다른 영역의 값이 필요하면 그 값만 인자로 받는다.
  - 이유: 연출의 순서와 읽는 데이터도 Logic처럼 한 곳에서 보인다. `ViewContext`를 통째로 받으면 시그니처가 영향 범위를 보여 주지 못한다.
- ViewSystem은 다른 ViewSystem을 호출하지 않는다.
  - 이유: 호출 안에 다른 ViewSystem의 동작이 숨으면 시그니처와 World의 호출 순서만 보고 언제 무엇이 일어나는지 알 수 없다.
- View 쪽 실행 상태(EntityIdentifier → View 매핑, 풀, 카메라 참조)는 영역별 **`~ViewContext` 인스턴스**에 두고, `ViewContext`가 이들을 모은다. ViewSystem도 static 필드에 상태를 두지 않는다.
  - 이유: 판을 다시 시작해도 이전 판의 View 매핑이 남지 않는다.
- 지속 상태는 매 프레임 엔티티 배열을 읽고, 그 순간의 일은 프레임 기록을 읽는다.
  - 이유: 지속 상태는 놓친 프레임이 있어도 따라잡고, 사건은 한 번만 재생된다.
- View 오브젝트는 살아 있는 엔티티와 비교해 생성·반납한다. 게임 루프에서 `Instantiate` / `Destroy`를 반복하지 않고 풀을 쓴다.
  - 이유: 게임 중 할당과 GC 스파이크를 막는다.

### UserInterface/
화면 UI를 MVP 구조로 표시하고 입력을 명령으로 전달한다.

- UI는 MVP 구조다. `UserInterface/`은 타입 역할별 하위 폴더로 나눈다. `Views/`에 `~View`를, `Presenters/`에 `~Presenter`를 둔다.
  - 이유: 타입의 접미사만 알면 그 파일의 위치를 안다.
- UI View는 버튼 입력을 이벤트로 발행하고 표시 메서드(`SetHealth` 등)를 가진다. Presenter는 MonoBehaviour이며, View의 이벤트를 구독해 명령 버퍼에 넣고, 자기를 연결한 쪽(씬의 대표인 World나 `Menu`, 앱 수명의 UI는 App)에게 필요한 데이터만 받아 View의 표시 메서드를 부른다. Model 변경 알림은 없고 연결한 쪽이 매 프레임 데이터를 넘긴다.
  - 이유: 게임 데이터를 아는 쪽은 Presenter뿐이라 화면을 바꿔도 로직이 안전하다. 매 프레임 넘기므로 변경 알림 이벤트가 필요 없다.
- View와 Presenter는 일대일이다.
  - 이유: Presenter 하나가 View 하나만 알면 데이터가 어디로 가는지 한 줄로 따라간다.
- 목록의 한 칸처럼 반복되는 표시 단위는 View가 소유하는 부품(`~Entry`)으로 두고 `Views/`에 둔다. 부품은 그 View만 만들고 다루며, 버튼 입력은 C# 이벤트 없이 소유한 View의 메서드를 불러 알린다. Presenter는 부품을 모른다.
  - 이유: 데이터가 Presenter → View → 부품 한 줄로만 흐르고, 입력도 View 한 곳의 이벤트로 모인다.

#### UI 계층 (uGUI)
- 모든 UI는 같은 계층을 가진다.
  - Root(이름 붙임): View 스크립트가 붙는다. width/height를 가진다.
    - Content: View가 켜고 끄는 오브젝트. Vertical Layout Group이 붙는다.
      - Header / Body / Footer: 셋 다 항상 있다. 각각 Layout Element로 자기 높이를 정한다.
  - 이유: HTML처럼 구조가 하나라서, 어느 UI든 같은 위치에서 같은 역할의 오브젝트를 찾는다.
- Canvas 밑에 Panel을 두고 그 아래에 UI Root들을 모은다. HUD도 요소(체력, 탄약, 낮밤 등)마다 Root와 View를 따로 둔다.
  - 이유: 요소마다 따로 켜고 끄고 옮길 수 있다.
- 레이아웃 그룹 바로 아래 자식에는 위치 트윈을 걸지 않고, 더 안쪽 자식에 건다.
  - 이유: 레이아웃 그룹이 바로 아래 자식의 위치를 덮어써서 트윈이 무시되거나 튄다.

### Core/
앱과 한 판의 데이터를 보관하고 실행 순서를 정한다.

- **App**은 앱에 하나뿐인 DDOL MonoBehaviour다. Menu와 World 씬이 함께 쓰고 씬이 아닌 앱의 수명을 따르는 일(볼륨과 설정 화면, 씬 전환·로딩·종료, 배경음악과 UI 소리 재생)을 맡는다. 한 씬의 수명을 따르는 일은 App에 두지 않는다. 요소별 상태와 처리는 `App.<요소>.cs`로 나눈다.
  - 이유: 두 씬에 같은 일이 있으면 같은 코드가 씬 수만큼 생기고 어긋난다. 씬의 일이 App에 붙으면 씬 전환을 알아채는 코드가 늘어 App이 커진다.
- **Unity의 FixedUpdate / Update / LateUpdate는 App만 받는다.** 씬의 대표는 이 함수들을 두지 않고, App이 부르는 함수(`Tick`, `ReadInput`, `Present` 등)만 둔다. App은 앱 수명의 일과 지금 씬의 대표를 정해진 순서로 부른다.
  - 이유: App과 씬이 각자 Update를 돌리면 호출 순서가 두 파일로 나뉘고, 순서가 Script Execution Order에 숨는다.
- 씬의 대표는 static `App.Instance`로 App에 닿는다. Start에서 자기를 App에 등록하고, OnDestroy에서 해제한다. 등록할 때 그 씬의 값(배경음악 Definition, 설정 화면을 열 수 있는지 등)을 넘긴다.
  - 이유: App은 씬보다 오래 살아 씬의 인스펙터로 가리킬 수 없다. 등록이 Start라야 에디터 부트스트랩(아래)이 만든 App에도 등록된다.
- 빌드는 Menu 씬에서 시작하고, App 프리팹은 Menu 씬에 배치한다. 이미 App이 있으면 새로 생긴 App은 자기를 없앤다(Menu로 돌아올 때). 에디터에서 다른 씬부터 Play하면, `App.Editor.cs`(파일 전체를 `#if UNITY_EDITOR`로 감쌈)가 씬을 불러온 직후 `AssetDatabase`로 App 프리팹을 찾아 만든다.
  - 이유: 빌드에는 에디터 전용 경로가 남지 않고, `Resources/`에 프리팹을 넣지 않아도 된다.

- **World**는 한 판의 데이터(엔티티 저장소, 기록 목록, 요청 큐, 명령 버퍼, 난수 생성기)를 가지고 틱 순서와 호출 순서를 관리하는 MonoBehaviour다. 요소별 상태와 처리는 `World.<요소>.cs`로, 한 판의 데이터는 `World.Data.cs`로 나눈다. 데이터 필드는 private으로 둔다.
  - 이유: 한 판의 상태가 객체 하나에 있으면 무엇이 판의 상태인지 한 곳에서 보인다. public 필드는 씬에 직렬화되고 다른 어셈블리가 닿을 수 있다.
- 새 판은 씬을 다시 불러와 시작한다. World를 다시 쓰거나 판 준비를 두 번 하지 않는다.
  - 이유: World가 씬과 함께 새로 생겨야 새 판에 이전 판의 값이 남지 않는다.
- 예외: `Editor/`의 Gizmo는 World가 `#if UNITY_EDITOR`로 연 통로(`Editor~` 프로퍼티, bool은 `IsEditor~`)로 World의 데이터를 읽을 수 있다. 통로는 필요한 값만 열고, Gizmo는 값을 바꾸지 않고 읽기만 한다.
  - 이유: 디버그 표시는 게임 결과를 바꾸지 않고, 통로가 빌드에 포함되지 않아 Logic과 View가 World의 데이터에 닿는 길이 생기지 않는다.
- 예외: `PlayModeTests/`의 테스트는 World가 `World.Test.cs`, App이 `App.Test.cs`(파일 전체를 `#if UNITY_INCLUDE_TESTS`로 감쌈)에 연 통로(`Test~` 프로퍼티, bool은 `IsTest~`)로 그 데이터를 읽고 쓸 수 있다. 통로는 테스트가 필요한 값만 연다.
  - 이유: 틱 순서와 앱 수명(설정 화면, 저장, 종료)에서 생기는 버그는 실제 `World`와 `App`을 돌려야 재현되고, 그러려면 테스트가 상황(엔티티, 진행 상태, 요청, 명령)을 직접 만들어야 한다. 통로가 빌드에 포함되지 않아 Logic과 View가 World와 App의 데이터에 닿는 길이 생기지 않는다.
- **World가 필요한 데이터만 꺼내 System, ViewSystem, Presenter에 넘긴다.** 이들은 World를 받지 않는다.
  - 이유: World를 통째로 받으면 시그니처만 보고 무엇을 읽고 쓰는지 알 수 없다.
- 한 판 안의 틱 순서와 호출 순서는 World가 관리한다. App은 World의 함수를 부를 뿐 판 안의 순서에 끼어들지 않는다.
  - 이유: 호출 순서가 결과를 바꾸므로, 순서를 정하는 곳이 한 곳이어야 한 화면에서 흐름을 따라갈 수 있다.
- World는 판이 있는 씬에 하나 배치한다. DDOL에 올리지 않고 씬과 함께 생기고 사라진다. 판이 없는 씬(메뉴 등)의 일과 앱 수명의 일을 World에 두지 않는다. App의 실행 상태(배경음악 등)가 판의 데이터를 읽어야 하면, App이 World를 부를 때 그 실행 상태를 인자로 넘긴다.
  - 이유: World가 가진 상태(한 판의 데이터, View 실행 상태, Presenter 연결)는 모두 판의 수명을 따른다. 씬보다 오래 살면 판과 상관없는 전역 일이 붙어 서비스처럼 커지고, 씬 전환을 알아채는 코드가 따로 필요해진다.
- **씬의 대표**는 씬마다 하나 배치해 그 씬의 일을 맡는 MonoBehaviour다. 판이 있는 씬의 대표는 World이고, 판이 없는 씬의 대표는 씬 이름을 딴다(예: `Menu`). 판이 없는 씬의 대표는 자기 씬의 Presenter를 연결하고 App이 부를 때 표시한다. 씬 전환과 로딩은 App이 한다. World를 만들지 않고 System을 부르지 않는다.
  - 이유: 틱이 없는 씬에는 호출 순서가 결과를 바꾸는 일이 없어 World에 둘 이유가 없다. 씬 오브젝트라 씬과 함께 사라져 이전 씬의 상태가 남지 않는다.
- 씬의 대표는 자기 씬에 하나뿐인 오브젝트(Presenter, 카메라·캔버스 같은 씬의 View)를 인스펙터 필드(`[SerializeField] private`)로 받는다. `FindAnyObjectByType` 등으로 씬을 검색하지 않는다. 앱 수명의 UI(설정 화면)도 그 씬의 대표가 받아 App에 등록할 때 넘긴다. 개수가 정해지지 않은 씬 배치 오브젝트(`~Authoring`, 정보 UI 앵커 같은 씬 배치 View)는 검색으로 모은다.
  - 이유: 무엇을 쓰는지 씬에서 바로 보이고, 빠지면 인스펙터에서 빈 칸으로 드러난다. 검색은 꺼진 오브젝트를 찾지 못하고, 같은 타입이 둘이면 아무거나 하나를 고른다. 개수가 정해지지 않은 것은 놓을 때마다 인스펙터에 연결해야 해서 오히려 빠뜨리기 쉽고, 모두 모으므로 "아무거나 하나"의 문제가 없다.
- 한 씬에 대표를 둘 두지 않는다(World와 `Menu`를 함께 두지 않는다).
  - 이유: 같은 씬을 두 곳이 돌리면 호출 순서가 다시 흩어진다.
- **로딩:** Core가 `~Definition`의 struct를 World에 대입한다. 판 시작 시 Core는 `~Authoring`마다 Transform 위치 등 씬 값을 채운 생성 요청을 큐에 넣고, 엔티티는 첫 틱의 `~SpawnSystem`이 만든다. Core는 World에 엔티티를 직접 넣지 않는다. System은 Definition과 Authoring을 참조하지 않는다.
  - 이유: 씬에 놓인 엔티티와 게임 중 생긴 엔티티가 같은 경로로 만들어진다. Logic이 에셋에 묶이지 않는다.

### Editor/
Gizmo, PropertyDrawer, Custom Editor 등 에디터 도구를 둔다. 플랫폼을 Editor 전용으로 두어 빌드에 포함되지 않는다.

- **Gizmo는 모두 `Editor/`에 두고 `[DrawGizmo]`로 그린다.** 씬에 배치한 값은 `~Authoring`을 읽어 그리고, 플레이 중의 Logic 값은 World의 에디터 전용 통로로 읽어 그린다.
  - 이유: 한 판의 데이터는 플레이 중에만 있어서 배치할 때 봐야 하는 값을 그릴 수 없다. Gizmo가 한 곳에 모이면 무엇을 어디에 둘지 고민하지 않는다.

### Tests/
System의 동작을 Unity Test Framework의 EditMode 테스트로 확인한다.

- **테스트는 System 하나를 단위로 쓴다.** 필요한 칸만 채운 `EntityStorage`와 큐를 만들어 `Tick`을 부르고, 칸과 큐와 기록의 결과를 확인한다. 테스트 클래스는 대상 System 이름에 `Tests`를 붙인다(`MeleeOwnerDeathSystemTests`).
  - 이유: System이 저장소와 큐만 받으므로 씬과 Definition 없이 돌릴 수 있다. 이름만 보고 무엇을 확인하는 테스트인지 안다.
- **버그는 고치기 전에 그 버그로 실패하는 테스트를 먼저 쓴다.** 테스트가 실패하는 것을 확인한 뒤 고치고, 통과를 확인한다.
  - 이유: 실패를 먼저 보지 않으면 테스트가 그 버그를 잡는지 알 수 없다. 남은 테스트가 같은 버그가 다시 생기는 것을 막는다.

### PlayModeTests/
World의 호출 순서와 씬(지형, NavMesh)에 달린 동작, 표시(ViewSystem, UI)의 동작을 PlayMode 테스트로 확인한다.

- **Logic은 System 하나로 드러나지 않는 버그만 PlayMode로 쓴다.** 실제 `World` 씬을 불러와 판이 준비되면 `Time.timeScale`을 0으로 두어 App의 자동 틱을 멈추고, 테스트가 `World.Tick`을 직접 부른다. 테스트 클래스는 확인하는 대상에 `Tests`를 붙인다(`WorldTickTests`).
  - 이유: 틱을 테스트가 부르면 몇 번째 틱에 무엇이 일어나는지 정확히 셀 수 있다. 한 틱 차이로 생기는 버그는 사람이 에디터에서 재현할 수 없다.
- **테스트가 놓는 엔티티의 값은 테스트가 직접 넣는다.** 씬의 배치나 Definition 수치에 기대지 않는다. 씬에서 쓰는 것은 지형, NavMesh처럼 테스트가 만들 수 없는 것뿐이다.
  - 이유: 씬을 고칠 때마다 테스트가 깨지면 테스트를 믿을 수 없다.
- **표시(ViewSystem, UI View, Presenter)의 버그는 World 씬 없이 그 대상 하나를 단위로 쓴다.** 테스트가 필요한 참조만 붙인 View 오브젝트를 직접 만들고 `Present`나 표시 메서드를 불러 결과(글자, 켜짐, 여백 등)를 확인한다. 인스펙터에서 받는 private 직렬화 필드는 리플렉션으로 채운다. 테스트 클래스는 대상 이름에 `Tests`를 붙인다(`PlayerHealthViewTests`). 트윈과 파티클처럼 프레임이 흘러야 하는 동작이 있어 PlayMode로 쓴다.
  - 이유: 표시는 System처럼 받은 데이터만으로 결과가 정해져, 씬과 프리팹 없이도 상황을 정확히 만들 수 있다. 씬의 UI 배치에 기대면 화면을 고칠 때마다 테스트가 깨진다.
