#!/bin/sh
# Unity 테스트를 돌리고 결과를 요약한다. 결과는 Tools/ReviewIssues/review_issue.py가 읽는 JSON으로도 남긴다.
#
# 사용:
#   sh Tools/Testing/run_tests.sh editmode [필터]
#   sh Tools/Testing/run_tests.sh playmode [필터]
#
# editmode: 사용자가 연 에디터에서 다시 컴파일하고(컴파일 오류, 분석기 경고 TV를 보여 준다) EditMode 테스트를 돌린다.
#           연결된 에디터가 없으면 멈춘다. 에디터를 띄우지 않는다.
# playmode: 테스트 전용 워크 트리를 원본에 맞춘 뒤(git이 무시하는 에셋 폴더와 스크립트 폴더) 배치 모드 에디터로 PlayMode 테스트를 돌린다.
#           사용자가 연 에디터는 건드리지 않는다. 몇 분 걸릴 수 있다.
#
# 결과: Project/Temp/TestResults/<mode>.json  [{ "fullName", "mode", "status", "message" }, ...]
# 종료 코드: 0 모두 통과, 1 실패한 테스트가 있음, 2 테스트를 돌리지 못함

MODE="$1"
FILTER="$2"
ROOT="$(git rev-parse --show-toplevel)" || exit 2
WORKTREE="$(cd "$ROOT/.." && pwd)/TopView-Game-Test"
RESULT_DIR="$ROOT/Project/Temp/TestResults"
EDITOR_LOG="$LOCALAPPDATA/Unity/Editor/Editor.log"
mkdir -p "$RESULT_DIR"

# 가져온 결과를 공통 JSON으로 바꾸고 요약을 출력한다. $1: 입력 형식(editmode-json, playmode-xml), $2: 입력 파일, $3: 모드 이름
summarize() {
    python - "$1" "$2" "$3" "$RESULT_DIR/$4.json" <<'PYEOF'
import json, sys, xml.etree.ElementTree as ElementTree
kind, source, mode, output = sys.argv[1:5]
sys.stdout.reconfigure(encoding='utf-8')
results = []
if kind == 'editmode-json':
    for line in open(source, encoding='utf-8'):
        if line.startswith('Results:'):
            for item in json.loads(line[len('Results:'):].strip()):
                results.append({'fullName': item['FullName'], 'mode': mode, 'status': item['Status'], 'message': (item['Message'] or '').strip()})
else:
    for case in ElementTree.parse(source).getroot().iter('test-case'):
        message = case.findtext('failure/message') or ''
        status = {'Passed': 'Passed', 'Failed': 'Failed', 'Skipped': 'Skipped'}.get(case.get('result'), case.get('result'))
        results.append({'fullName': case.get('fullname'), 'mode': mode, 'status': status, 'message': message.strip()})
json.dump(results, open(output, 'w', encoding='utf-8'), ensure_ascii=False, indent=1)
failed = [item for item in results if item['status'] != 'Passed']
print('%s: %d개 중 %d개 통과, %d개 실패' % (mode, len(results), len(results) - len(failed), len(failed)))
for item in results:
    line = '  %-7s %s' % (item['status'], item['fullName'])
    if item['message']:
        line += '\n          ' + ' | '.join(part.strip() for part in item['message'].splitlines() if part.strip())
    print(line)
print('결과 파일: %s' % output)
sys.exit(0 if results and not failed else 1)
PYEOF
}

wait_editor_ready() {
    for attempt in 1 2 3 4 5 6 7 8 9 10 11 12; do
        unity status 2>/dev/null | grep -iF "$(cygpath -w "$ROOT/Project")" | grep -q "ready" && return 0
        sleep 5
    done
    return 1
}

case "$MODE" in
editmode)
    if ! unity status 2>/dev/null | grep -qiF "$(cygpath -w "$ROOT/Project")"; then
        echo "연결된 에디터가 없어 EditMode 테스트를 돌리지 못했다. 에디터를 연 뒤 다시 돌린다."
        exit 2
    fi
    LOG_START=$(wc -l < "$EDITOR_LOG")
    unity recompile --project-path "$ROOT/Project" --timeout 300 2>&1 | grep -v "^severity" | sed '/^$/d'
    # recompile 결과 표가 분석기 경고를 놓칠 때가 있어 Editor.log에서 다시 찾는다
    COMPILE_ERRORS=$(tail -n +"$LOG_START" "$EDITOR_LOG" | grep "error CS" | sort -u)
    tail -n +"$LOG_START" "$EDITOR_LOG" | grep "error CS\|warning TV" | grep -v "?" | sort -u
    if [ -n "$COMPILE_ERRORS" ]; then
        echo "컴파일 오류가 있어 테스트를 돌리지 않았다."
        exit 2
    fi
    # 다시 컴파일한 직후에는 도메인 리로드와 겹쳐 명령이 실패하므로 준비될 때까지 기다린다
    wait_editor_ready || { echo "에디터가 준비되지 않아 테스트를 돌리지 못했다."; exit 2; }
    sleep 3
    RAW="$RESULT_DIR/editmode.raw.txt"
    unity cmd --project-path "$ROOT/Project" test run --mode editor --filter "$FILTER" > "$RAW" 2>&1
    grep -q "^Results:" "$RAW" || { cat "$RAW"; echo "테스트 결과를 받지 못했다."; exit 2; }
    summarize editmode-json "$RAW" EditMode editmode
    ;;
playmode)
    [ -d "$WORKTREE/Project" ] || { echo "테스트 전용 워크 트리 $WORKTREE 가 없다."; exit 2; }
    for path in "Assets/Local Assets" "Packages/com.occasoftware.super-simple-skybox" "Assets/Scripts"; do
        MSYS_NO_PATHCONV=1 robocopy "$(cygpath -w "$ROOT/Project/$path")" "$(cygpath -w "$WORKTREE/Project/$path")" /MIR /MT:16 /R:1 /W:1 /NFL /NDL /NP /NJH /NJS > /dev/null
        # robocopy는 0~7이 성공이다
        [ $? -lt 8 ] || { echo "워크 트리 동기화에 실패했다: $path"; exit 2; }
    done
    XML="$RESULT_DIR/playmode.xml"
    rm -f "$XML"
    if [ -n "$FILTER" ]; then
        unity test "$(cygpath -m "$WORKTREE/Project")" --mode PlayMode --filter "$FILTER" -o "$(cygpath -m "$XML")" --non-interactive > "$RESULT_DIR/playmode.raw.txt" 2>&1
    else
        unity test "$(cygpath -m "$WORKTREE/Project")" --mode PlayMode -o "$(cygpath -m "$XML")" --non-interactive > "$RESULT_DIR/playmode.raw.txt" 2>&1
    fi
    [ -f "$XML" ] || { tail -20 "$RESULT_DIR/playmode.raw.txt"; echo "테스트 결과 파일이 없다. 컴파일 오류일 수 있다."; exit 2; }
    summarize playmode-xml "$XML" PlayMode playmode
    ;;
*)
    echo "사용: sh Tools/Testing/run_tests.sh editmode|playmode [필터]"
    exit 2
    ;;
esac
