export const meta = {
  name: 'topview-game-code-review',
  description: 'TopView 게임 코드 A(데이터·저장소)·B(게임 로직) 영역을 4개 리뷰 에이전트로 나눠 집중 리뷰',
  phases: [{ title: 'Review', detail: 'A1 World·App, A2 엔티티 데이터, B1 전투, B2 행동·진행' }],
}

const ROOT = 'Project/Assets/Scripts'

const AREAS = [
  {
    key: 'A1-world-app',
    name: 'A1. World·App',
    scope: [`${ROOT}/Core/ (단, ${ROOT}/Core/PlayerActions.cs는 Input System 자동 생성 파일이므로 제외)`],
    focus: 'World partial 파일들의 틱 순서와 System 호출 순서, 라운드 시작·재시작 시 상태 초기화 누락, 저장소·큐 생명주기, App(씬 전환·설정·오디오·음악)의 상태 전이와 이벤트 구독/해제, 일시정지와 고정 시간 간격 처리.',
  },
  {
    key: 'A2-entity-data',
    name: 'A2. 엔티티 데이터',
    scope: [`${ROOT}/Entities/`, `${ROOT}/Components/`, `${ROOT}/Commands/`, `${ROOT}/Records/`, `${ROOT}/Requests/`],
    focus: 'EntityStorage(AoS Entity + ComponentMask + generation)의 생성·삭제·빈 칸 재사용·구간(종류별 칸 범위) 처리, generation 검사 누락으로 죽은/재사용된 칸을 잘못 참조하는 경우, EntityArchetype 마스크와 실제로 채우는 컴포넌트의 불일치, 마스크 비트 누락·중복, 큐/요청/명령 구조의 용량 초과와 비우기 시점.',
  },
  {
    key: 'B1-combat',
    name: 'B1. 전투',
    scope: [`${ROOT}/Systems/Attack/`, `${ROOT}/Systems/Melee/`, `${ROOT}/Systems/Projectile/`, `${ROOT}/Systems/Weapon/`, `${ROOT}/Systems/Vitals/`, `${ROOT}/Utilities/`],
    focus: '근접 판정·직선/포물선 투사체·무기·피해·체력·사망 처리. 같은 틱에 두 번 맞거나 한 번도 안 맞는 경우, 소유자가 죽은 뒤 남은 판정/투사체, 탄약·발사 간격·발 시점의 off-by-one(틱 단위), 진영(플레이어/적) 구분 오류, 플레이어 무기 경로와 적 공격 경로(EnemyAttackComponent·공격 목록)가 같은 일을 다르게 처리해 어긋나는 곳.',
  },
  {
    key: 'B2-action-progress',
    name: 'B2. 행동·진행',
    scope: [`${ROOT}/Systems/Action/`, `${ROOT}/Systems/Movement/`, `${ROOT}/Systems/Spawning/`, `${ROOT}/Systems/Game/`, `${ROOT}/Systems/Loot/`, `${ROOT}/Systems/Upgrade/`, `${ROOT}/Definitions/`, `${ROOT}/Authoring/`, `${ROOT}/Persistence/`],
    focus: '플레이어·적 행동 상태 전이와 상태 타이머, 이동·충돌·막힘 해소, 스포너·플레이어 감지기 엔티티(연결 번호 SpawnerLink)와 적 생성, 승패 규칙, 전리품 생성·수명·획득, 강화 적용, Definition 값과 저장 데이터 읽기/쓰기. 상태 전이 누락, 타이머 리셋 누락, 0 나누기·NaN, 배열 범위, 저장 데이터 버전/누락 처리.',
  },
]

const FINDINGS_SCHEMA = {
  type: 'object',
  properties: {
    filesRead: { type: 'array', items: { type: 'string' }, description: '실제로 끝까지 읽은 파일 경로' },
    findings: {
      type: 'array',
      items: {
        type: 'object',
        properties: {
          file: { type: 'string', description: '저장소 루트 기준 경로' },
          line: { type: 'integer' },
          category: { type: 'string', enum: ['bug', 'inconsistency', 'rule', 'cleanup'] },
          severity: { type: 'string', enum: ['high', 'medium', 'low'] },
          confidence: { type: 'string', enum: ['high', 'medium', 'low'] },
          title: { type: 'string', description: '한국어 한 줄' },
          description: { type: 'string', description: '한국어. 무엇이 왜 잘못인지' },
          failureScenario: { type: 'string', description: '한국어. 구체적인 상황 → 잘못된 결과' },
          evidence: { type: 'string', description: '근거가 되는 코드 위치들(file:line)과 짧은 인용' },
        },
        required: ['file', 'line', 'category', 'severity', 'confidence', 'title', 'description', 'failureScenario', 'evidence'],
      },
    },
  },
  required: ['filesRead', 'findings'],
}

const COMMON = `
너는 Unity 6 탑뷰 게임(TopView) 코드의 리뷰어다. 저장소 루트는 현재 작업 디렉터리다.
배경:
- 게임 로직은 고정 시간 간격 0.02초(50Hz) 틱으로 돈다.
- 엔티티는 공유 저장소 EntityStorage에 있다. 칸 하나 = Entity struct 하나(AoS), 컴포넌트 필드 + mask(ComponentMask, [Flags] enum : ulong) + generation. mask None = 빈 칸. 종류별 구간과 구간별 빈 칸 목록이 있다. 종류별 마스크 상수는 Entities/EntityArchetype에 있다.
- System은 처리하는 마스크와 읽고 쓰는 컴포넌트를 storage 인자의 문서 주석에 적는다.
- 구조 리팩터링은 이제 중단됐다. 옛 방식과 새 방식이 섞인 곳은 "진행 중"이 아니라 최종 상태다. 두 경로가 같은 일을 다르게 처리해 어긋나거나, 주석·문서가 앞으로 할 단계를 전제로 쓰인 곳은 지적 대상(category: inconsistency)이다.
- 프로젝트 규칙은 .claude/rules/에 있다. 규칙 위반은 category: rule로 낮은 우선순위로만 적는다.

엄격한 지침:
- 읽기만 한다. 파일을 만들거나 고치지 않는다. 빌드·Unity 명령을 실행하지 않는다.
- 맡은 범위의 .cs 파일을 모두 끝까지 읽는다. 범위 밖 코드는 호출·데이터 흐름을 확인하려고 따라가 읽어도 되지만, 지적은 맡은 범위 안의 위치에만 단다.
- 우선순위: 실제로 잘못 동작하는 버그(bug) > 경로 간 불일치(inconsistency) > 규칙(rule) > 정리(cleanup). 스타일 취향은 적지 않는다.
- 각 지적은 실제 코드를 근거로 구체적 상황과 잘못된 결과를 적는다. 추측이면 confidence를 낮추고 그 이유를 description에 쓴다. 확인 못 한 것을 확신처럼 쓰지 않는다.
- Unity 에디터 쪽 설정(씬, 프리팹 값)에 따라 달라지는 것은 그 전제를 밝힌다.
- 글은 한국어로 쓴다.
`

phase('Review')
const results = await parallel(AREAS.map(a => () =>
  agent(`${COMMON}
너의 영역: ${a.name}
맡은 범위:
${a.scope.map(s => '- ' + s).join('\n')}
특히 볼 것: ${a.focus}

범위의 파일 목록을 먼저 확인하고, 모두 읽은 뒤 지적을 정리해라. 지적 수에 제한은 없지만, 근거 없는 지적은 넣지 않는다.`,
    { label: `review:${a.key}`, phase: 'Review', schema: FINDINGS_SCHEMA })
    .then(r => r ? { area: a.name, ...r } : null)
))

const ok = results.filter(Boolean)
const failed = AREAS.filter((a, i) => !results[i]).map(a => a.name)
if (failed.length) log(`실패한 영역: ${failed.join(', ')}`)
return { areas: ok, failed }
