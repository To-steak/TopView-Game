# 테스트 실행 도구

`run_tests.sh`는 Unity 테스트를 돌리고 결과를 요약한다. 결과는 이슈 도구(`Tools/ReviewIssues/review_issue.py`)가 읽는 JSON으로도 남긴다.

```sh
sh Tools/Testing/run_tests.sh editmode [필터]
sh Tools/Testing/run_tests.sh playmode [필터]
```

- 결과: `Project/Temp/TestResults/<mode>.json`
- 종료 코드: 0 모두 통과, 1 실패한 테스트가 있음, 2 테스트를 돌리지 못함

## 필요한 것

- Python 3: 테스트 결과를 JSON으로 요약한다. 표준 라이브러리만 쓴다
- `unity` 명령(Unity CLI): 에디터에 다시 컴파일과 테스트를 시키고, 배치 모드 에디터를 띄운다
- Git, Windows의 `robocopy`: PlayMode 워크 트리를 원본에 맞춘다

게임을 열거나 에디터의 Test Runner로 테스트를 돌리는 데는 필요 없다.

## EditMode

사용자가 연 에디터에서 다시 컴파일하고(컴파일 오류, 분석기 경고를 보여 준다) 돌린다. 연결된 에디터가 없으면 돌리지 않고, 에디터를 띄우지도 않는다.

## PlayMode

테스트 전용 워크 트리 `../TopView-Game-Test`에서 배치 모드 에디터로 돌린다. 사용자가 연 에디터는 건드리지 않는다. 몇 분 걸리므로 백그라운드로 돌린다.

- 이유: 다른 폴더의 배치 모드 에디터는 사용자의 에디터와 씬을 끊지 않는다. 같은 프로젝트 폴더는 에디터 둘이 동시에 열 수 없다.
- 직접 돌릴 때는 절대 경로를 준다: `unity test "C:/Users/duddn/Repository/TopView-Game-Test/Project" --mode PlayMode`. 상대 경로를 주면 같은 이름의 다른 프로젝트와 헷갈려 실패한다.
- 워크 트리는 지우지 않고 계속 쓴다. `Library/`는 처음 한 번 원본에서 복사했고, 그 뒤로는 워크 트리의 Unity가 자기 `Assets/`를 보고 맞춘다.
- 스크립트는 돌리기 전에 워크 트리를 원본에 맞춘다.
  - git이 무시하는 에셋 폴더(`Project/Assets/Local Assets/`, `Project/Packages/com.occasoftware.super-simple-skybox/`)와 스크립트 폴더를 `robocopy /MIR`로 맞춘다.
  - 패키지 목록(`Project/Packages/manifest.json`, `packages-lock.json`)을 복사한다.
  - 그래서 커밋하지 않은 코드와 패키지 변경도 함께 맞춰진다.
