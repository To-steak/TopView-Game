# TopView Game
<!--
TODO: 위 링크 줄 아래에 한 줄 소개(장르, 플레이 방식), 영상 길이 표기(트레일러 1:20, 플레이 영상 8:20), Notion 포트폴리오 링크
- 역할: 양쪽 공통. 무엇을 만들었는지와 바로 볼 수 있는 결과물을 맨 위에 둔다.
- 출처: "AI Game Maker" - "아이디어를 실제로 플레이할 수 있는 게임으로 만드는 일"
- 출처: Q7 - 분량보다 대표 사례 중심으로 간결하게
-->
[![TopView Game 트레일러](Docs/Image/YouTube_Thumbnail.png)](https://youtu.be/7fyoxN1xmZw)

> 적을 물리치고 강화를 하며 최종적으로 보스를 쓰러뜨리는 탑다운 액션 게임을 기획하고 CLAUDE 함께 작업하였습니다.

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


## 프로젝트 세 줄 요약
<!--
TODO: 세 줄 요약 - 무엇을 만들었는지, AI와 어떻게 나눠 일했는지, 무엇으로 결과를 검증했는지
- 역할: 양쪽 공통. Notion과 같은 세 줄을 써도 된다.
- 출처: "탁월한 인턴, 성장 가이드" - "바쁘신 분을 위해 먼저, 딱 세 줄로"
- 출처: 성장 가이드 01 - "무언가를 끝까지 만들어 내고 그 과정을 자기 말로 설명할 수 있는 사람"
-->
<img src="Docs/Image/counter_1_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"> Unity 6로 탑다운 액션 게임을 기획부터 구현, 트레일러 제작, 배포까지 혼자 완성했습니다.

<img src="Docs/Image/counter_2_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"> 기획·설계와 플레이 확인은 제가 맡고 구현과 리뷰는 AI(Claude)에게 맡겼으며, 그 경계를 규칙 문서([CLAUDE.md](/CLAUDE.md))로 정해 AI가 규칙과 어긋나면 먼저 묻게 했습니다.

<img src="Docs/Image/counter_3_24dp_C0C0C0_FILL0_wght400_GRAD0_opsz24.svg" align="absmiddle"> AI가 쓴 코드는 직접 만든 정적 분석기([Roslyn](/Tools/TopView.Analyzers/)), 구현과 분리된 리뷰 에이전트([Claude Code Workflow](/Tools/CodeReview/README.md)), 지적마다 재현 테스트([Unity Test Framework](/Tools/Testing/README.md))를 거쳐 확인하고, 재현된 버그만 이슈([GitHub Issues](/Tools/ReviewIssues/README.md))로 남겼습니다.

## 개발 과정
<!--
TODO: 항목마다 한두 줄 요약 + Notion의 해당 사례 링크 + 저장소 안 근거(파일, 이슈, 커밋) 링크
- 역할: 원본은 Notion. 여기서는 길게 쓰지 않는다. 자세한 이야기는 Notion에만 둔다.
- 항목:
  - 왜 만들었나: 동기와 기획 의도
    출처: 성장 가이드 "미리 알아 두면 좋은 것" 2 - "그 아이디어가 어떤 경험을 기반으로 나왔고, 어떻게 구체화했는지"
  - AI와 사람의 역할 분담: 근거로 CLAUDE.md, .claude/rules/ 링크
    출처: "문제 해결 역량" - AI 리더십 / 성장 가이드 02 AI 활용 테스트 - "AI에게 맡긴 일과 내가 직접 판단한 일을 구분"
  - 문제를 나누고 정리한 방법: 근거로 리팩터링 PR이나 이슈 링크
    출처: "문제 해결 역량" - 구조화 능력
  - 새로 배워서 적용한 것
    출처: "문제 해결 역량" - 학습 실행력
  - 직무 경계를 넘어 완성한 것: 기획, 트레일러, 썸네일, 배포
    출처: "우대 사항" - "직무의 경계를 넘어 결과물을 완성해 본 경험"
  - 의사결정 기록: 근거로 결정이 담긴 커밋이나 문서 링크
    출처: 성장 가이드 02 - "왜 그렇게 결정했는지와 어떤 흐름으로 생각했는지" / Q7 - "어떤 고민을 담았는가"
-->

## AI가 일할 수 있는 환경
<!--
TODO: AI가 직접 일하고 스스로 확인할 수 있게 만든 도구와 사용법
- 역할: 원본은 README. Notion에는 요약과 이 섹션 링크만 둔다.
- 예: unity recompile로 컴파일 오류 확인, Tools/Testing/run_tests.sh로 EditMode/PlayMode 테스트, 테스트 전용 워크 트리(../TopView-Game-Test), 프로젝트 스킬(.claude/skills/review-to-issues)
- 출처: 현직자 게임 기획 Q1 - "AI가 실제로 일할 수 있는 환경을 만들어 주는 게 중요"
- 출처: 현직자 게임 기획 Q2 - "AI가 스스로 일하는 환경부터 만들어 보세요"
- 출처: 성장 가이드 03 - "반복되는 일을 시스템으로 만들어 두는 습관"
-->

## 검증 체계
<!--
TODO: AI가 만든 결과를 어떻게 검증했는지, 저장소 안의 증거 링크와 함께
- 역할: 원본은 README. Notion에는 요약과 이 섹션 링크만 둔다.
- 예:
  - 정적 분석기(Tools/TopView.Analyzers, TV0001~): 꼭 지킬 규칙을 검사기로 막음, 예시 문서(Tools/TopView.Analyzers.Tests/Examples/)가 곧 테스트
  - 코드 리뷰 → 지적마다 테스트로 재현 → reproduced만 이슈로 남김 (Docs/Reviews/, Tools/CodeReview/, 리뷰 이슈 링크)
  - 한 틱 차이 버그처럼 사람이 재현할 수 없는 것은 테스트로만 확인
  - 리뷰 도구 측정 기록(Docs/ReviewBenchmark/)
- 출처: 현직자 프로그래밍 Q1 팁 - "다른 AI에게 리뷰를 맡겨 보세요", "lint 같은 검사기로 막아 두는 게 확실해요", "완료 기준부터 정해 두는 편"
- 출처: 현직자 게임 기획 Q2 - "결과물을 검증하는 절차까지 함께 설계해 두면 좋아요"
-->

## 기술 구조
<!--
TODO: 어셈블리 구성(TopView.Core, Presentation, UserInterface 등)과 주요 설계를 간단히, 필요하면 다이어그램
- 역할: 원본은 README.
- 이전 README의 FSM, Mediator, Observer, ScriptableObject, Object Pooling 설명 중 현재 코드와 맞는 것만 남긴다.
- 출처: Q5 - "Unity, Unreal 등 상용 엔진을 다뤄보았거나 프로그래밍 기본 지식이 있다면" 도움이 됨
- 출처: "우대 사항" - "프로그래밍에 대한 이해를 바탕으로 AI와 능숙하게 협업하고 결과물을 만들어 본 경험"
-->

## 저장소 구성
<!--
TODO: 최상위 폴더가 무엇을 담는지 표로 (Project/, Tools/, Docs/, .claude/, .github/)
- 역할: 원본은 README. 코드를 처음 연 사람이 증거를 찾아갈 수 있게 하는 지도.
- 출처: 공고에 직접 없음. "검증 체계"와 "개발 과정"의 근거 링크를 따라가기 쉽게 하려는 항목
-->

## 실행 방법
<!--
TODO: 플레이 방법(Release 다운로드)과 프로젝트 여는 방법(Unity 6000.3.27f2, URP), 조작키
- 역할: 원본은 README.
- 출처: 공고에 직접 없음. "AI Game Maker" - 플레이할 수 있는 결과물을 직접 확인하게 하는 항목
-->

## License
<!--
TODO: 사용한 에셋과 라이선스 (이전 README의 Quarter View 3D Action Assets Pack 항목)
- 역할: 원본은 README.
- 출처: 공고에 없음. 저장소 공개에 필요한 항목
-->
