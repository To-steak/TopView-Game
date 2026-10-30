---
paths:
  - "Project/Assets/Scripts/**/*.cs"
---

# C# 코드 규칙

규칙마다 `이유:` 줄은 그 규칙이 없으면 무엇이 깨지는지를 적는다.

## 네임스페이스
- `<접두사>.*`, 폴더 경로와 일치시킨다. 접두사는 `CLAUDE.md`의 프로젝트 값이다. 예: `Presentation/Player/ViewSystems/` → `<접두사>.Presentation.Player.ViewSystems`.
  - 이유: 파일 위치만 보고 네임스페이스를 알고, 네임스페이스만 보고 파일을 찾는다.

## 이름
- 단어를 줄여 쓰지 않는다 (`cnt`, `idx`, `pos` 금지). 반복 변수도 `i`, `j` 대신 `index`, `enemyIndex`. 외부 API 이름은 예외.
  - 이유: 줄임말은 사람마다 다르게 줄여 읽기와 검색이 어려워진다. 중첩 반복에서 `i`, `j`는 무엇을 도는지 드러내지 않는다.
- 머리글자 약어도 쓰지 않는다 (`HP` 대신 `Health`, `ID` 대신 `Identifier`, `UI` 대신 `UserInterface`).
  - 이유: 줄임말과 같다. 약어의 대소문자 표기(`Id`, `ID`)도 흔들리지 않는다.

| 대상 | 규칙 | 예시 |
|---|---|---|
| 타입, 메서드, 프로퍼티 | PascalCase | `MoveSystem`, `Tick()` |
| public 필드 | camelCase | `currentPosition` |
| private 필드 | `_camelCase` | `_runner` |
| 지역 변수, 매개변수 | camelCase | `targetEnemy` |
| 상수 | UPPER_SNAKE_CASE | `MAX_HEALTH` |
| bool | `is`, `has`, `can` 접두사 | `isAlive` |

- 이유: 이름만 보고 필드인지, 지역 변수인지, 상수인지 구분한다. bool은 조건문이 문장처럼 읽힌다.

접미사: `~System`, `~ViewSystem`, `~Entity`, `~Storage`, `~Record`, `~Request`, `~Command`, `~Component`, `~Definition`, `~Authoring`, `~View`, `~Presenter`, 시간 값은 `~Ticks`.
- 이유: 이름만 보고 그 타입의 역할과 따르는 규칙을 안다. `~Ticks`는 단위가 초가 아니라 틱임을 드러낸다.

## 스타일
- `var` 금지. 타입을 명시한다.
  - 이유: 타입이 보이지 않으면 struct 복사인지 참조인지 코드만 보고 구분할 수 없다.
- 파일 하나에 타입 하나, 파일 이름은 타입 이름과 같다. 중첩 타입 금지.
  - 이유: 파일 이름으로 타입을 찾는다. Unity는 MonoBehaviour와 ScriptableObject를 파일 이름으로 연결한다.
- 예외: 한 타입을 역할별로 나눌 때는 `partial`로 파일을 나눈다. 본 파일은 `타입이름.cs`, 나머지는 `타입이름.역할.cs`(역할은 PascalCase)로 짓고 같은 폴더에 둔다. 예: `Runner.cs`, `Runner.Gizmos.cs`. 에디터 전용 파일은 파일 전체를 `#if UNITY_EDITOR`로 감싼다.
  - 이유: 큰 타입을 나눠도 같은 이름으로 묶여 한 번에 찾는다. 에디터 코드가 빌드에 섞이지 않는다.
- 익명 함수(람다식 `() => …`, `x => …`, 익명 메서드 `delegate { … }`)를 쓰지 않는다. 콜백이 필요하면 메서드를 만들고 이름을 넘긴다. 예: `tween.OnComplete(EnableInteraction)`. 식 본문 멤버(`public int Count => …;`)는 익명 함수가 아니므로 허용한다.
  - 이유: 클로저는 숨은 할당과 캡처를 만들고, 콜백이 이름 없이 숨어 흐름을 따라가기 어렵다.
- 중괄호는 다음 줄에 연다 (Allman).
  - 이유: 형식이 하나여야 diff가 형식 차이로 흐려지지 않는다.
- 멤버 순서: public 필드 → `[SerializeField] private` 필드 → private 필드 → const → public 메서드 → private 메서드.
  - 이유: 어느 파일이든 같은 종류의 멤버를 같은 위치에서 찾는다.
- 엔티티 데이터는 struct + public 필드. 배열 원소 struct는 **반드시 `ref`로 접근**한다 (`ref EnemyEntity enemy = ref enemies[index];`). 매개변수는 수정하면 `ref`, 읽기만 하면 `in`.
  - 이유: 배열 원소를 값으로 꺼내면 복사본을 고쳐서 원본이 바뀌지 않는 버그가 생긴다. `in`/`ref`로 수정 여부가 시그니처에 드러난다.

## Unity
- 직렬화 필드 이름을 바꿀 때는 `[FormerlySerializedAs("이전이름")]`을 붙인다.
  - 이유: 붙이지 않으면 에디터에서 입력한 값이 사라진다.
- 입력은 새 Input System만 쓴다. 구 `UnityEngine.Input` API를 쓰지 않는다.
  - 이유: 같은 입력을 두 경로로 읽으면 바인딩과 동작이 어긋난다.

## 주석
- public 메서드: 한국어 XML 문서 주석. `<summary>` 필수, 매개변수가 있으면 `<param>`, 반환값이 있으면 `<returns>`.
  - 이유: 호출하는 쪽에서 IDE 툴팁으로 의도를 본다.
- public 필드: 선언 바로 윗줄에 한국어 한 줄 주석.
  - 이유: 데이터 struct의 필드 설명이 곧 설계 문서다. 단위와 의미(예: 음수면 없음)를 남긴다.
- private 멤버와 지역 변수는 필요할 때만 단다.
  - 이유: 이름으로 드러나는 것을 반복하는 주석은 코드와 함께 고쳐지지 않아 틀려진다.
