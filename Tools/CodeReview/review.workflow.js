export const meta = {
  name: 'topview-code-review',
  description: 'TopView 코드를 영역별 리뷰 에이전트로 나눠 읽고, 정해진 JSON 형식으로 지적을 모은다',
  whenToUse: '코드 리뷰를 해서 그 결과를 Docs/Reviews/에 JSON으로 남기고 이슈로 이어 갈 때',
  phases: [{ title: 'Review', detail: '영역마다 리뷰 에이전트 하나' }],
}

// 이 스크립트는 바뀌지 않는 부분(결과 형식, 공통 지침)만 가진다.
// 리뷰마다 바뀌는 값은 실행할 때 args로 받는다.
//   args.date     리뷰한 날짜 (예: '2026-11-14')
//   args.commit   리뷰한 커밋 (예: '45ba8812'). 위치 링크가 이 커밋의 줄을 가리킨다
//   args.context  이번 리뷰의 상황 설명. 없으면 빈 문자열
//   args.areas    영역 목록. 영역마다 { key, name, scope: [폴더나 파일 경로], focus: 특히 볼 것 }
// 결과에는 args를 그대로 함께 남겨, 무엇을 어떻게 리뷰했는지 결과만 보고 알 수 있게 한다.

if (!args || !args.date || !args.commit || !Array.isArray(args.areas) || args.areas.length === 0) {
  throw new Error('args에 date, commit, areas(한 개 이상)가 있어야 한다.')
}

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
- 게임 로직은 고정 시간 간격 틱으로 돈다. 틱 길이는 Project Settings > Time > Fixed Timestep이다.
- 엔티티는 공유 저장소 EntityStorage에 있다. 칸 하나 = Entity struct 하나(AoS), 컴포넌트 필드 + mask(ComponentMask) + generation. mask None = 빈 칸. 종류별 마스크 상수는 Entities/EntityArchetype에 있다.
- System은 처리하는 마스크와 읽고 쓰는 컴포넌트를 storage 인자의 문서 주석에 적는다.
- 프로젝트 규칙은 .claude/rules/에 있다. 규칙 위반은 category: rule로 낮은 우선순위로만 적는다.
${args.context ? '\n이번 리뷰의 상황:\n' + args.context + '\n' : ''}
엄격한 지침:
- 읽기만 한다. 파일을 만들거나 고치지 않는다. 빌드·Unity 명령을 실행하지 않는다.
- 맡은 범위의 .cs 파일을 모두 끝까지 읽는다. 범위 밖 코드는 호출·데이터 흐름을 확인하려고 따라가 읽어도 되지만, 지적은 맡은 범위 안의 위치에만 단다.
- 우선순위: 실제로 잘못 동작하는 버그(bug) > 경로 간 불일치(inconsistency) > 규칙(rule) > 정리(cleanup). 스타일 취향은 적지 않는다.
- 각 지적은 실제 코드를 근거로 구체적 상황과 잘못된 결과를 적는다. 추측이면 confidence를 낮추고 그 이유를 description에 쓴다. 확인 못 한 것을 확신처럼 쓰지 않는다.
- Unity 에디터 쪽 설정(씬, 프리팹 값)에 따라 달라지는 것은 그 전제를 밝힌다.
- 글은 한국어로 쓴다.
`

phase('Review')
const results = await parallel(args.areas.map(area => () =>
  agent(`${COMMON}
너의 영역: ${area.name}
맡은 범위:
${area.scope.map(path => '- ' + path).join('\n')}
${area.focus ? '특히 볼 것: ' + area.focus : ''}

범위의 파일 목록을 먼저 확인하고, 모두 읽은 뒤 지적을 정리해라. 지적 수에 제한은 없지만, 근거 없는 지적은 넣지 않는다.`,
    { label: `review:${area.key}`, phase: 'Review', schema: FINDINGS_SCHEMA })
    .then(result => result ? { area: area.name, ...result } : null)
))

const areas = results.filter(Boolean)
const failed = args.areas.filter((area, index) => !results[index]).map(area => area.name)
if (failed.length) log(`실패한 영역: ${failed.join(', ')}`)
return { date: args.date, commit: args.commit, context: args.context || '', request: args.areas, areas, failed }
