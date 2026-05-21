import {
    ConnectionStatus,
    ON_CLEAR_OCCURENCES,
    ON_CONNECTION_STATUS_CHANGE,
    ON_NEW_RAW_OCCURENCE,
} from "../types/common/ErisEvents"

const DEFAULT_PORT = 10470
const RECONNECT_DELAY_MS = 3000

// Latest status, exposed so the status UI element can read it on mount
// without waiting for the next state transition.
let lastStatus: ConnectionStatus = { status: "disconnected", reconnectAt: null }

export function getConnectionStatus(): ConnectionStatus {
    return lastStatus
}

function dispatchStatus(detail: ConnectionStatus) {
    lastStatus = detail
    document.dispatchEvent(
        new CustomEvent(ON_CONNECTION_STATUS_CHANGE, { detail }),
    )
}

// Tracks the game-side river identity. The game sends `{ type: "session-started",
// sessionGuid, ... }` on every fresh socket subscription. A change in sessionGuid
// means the game restarted while the UI stayed open — the existing list refers to
// a dead river, so we clear it. Same guid means reconnect-to-same-river (network
// blip, page reload while game runs); list is preserved.
let lastSessionGuid: string | null = null

function handleSessionStarted(sessionGuid: string) {
    if (lastSessionGuid !== null && lastSessionGuid !== sessionGuid) {
        console.log(`[Eris] New game session ${sessionGuid}; clearing list`)
        document.dispatchEvent(new CustomEvent(ON_CLEAR_OCCURENCES))
    }
    lastSessionGuid = sessionGuid
}

export function connectToDebugServer(port: number = DEFAULT_PORT) {
    let ws: WebSocket | null = null
    let reconnectTimer: ReturnType<typeof setTimeout> | null = null

    function connect() {
        ws = new WebSocket(`ws://127.0.0.1:${port}`)

        ws.onopen = () => {
            console.log(`[Eris] Connected to debug server on port ${port}`)
            dispatchStatus({ status: "connected" })
        }

        ws.onmessage = (event: MessageEvent) => {
            try {
                const data = JSON.parse(event.data)

                if (data?.type === "session-started") {
                    handleSessionStarted(data.sessionGuid)
                    return
                }

                const customEvent = new CustomEvent(ON_NEW_RAW_OCCURENCE, {
                    detail: data,
                })
                document.dispatchEvent(customEvent)
            } catch (err) {
                console.error("[Eris] Failed to parse message:", err)
            }
        }

        ws.onclose = () => {
            console.log(
                `[Eris] Disconnected. Reconnecting in ${RECONNECT_DELAY_MS / 1000}s...`
            )
            dispatchStatus({
                status: "disconnected",
                reconnectAt: Date.now() + RECONNECT_DELAY_MS,
            })
            scheduleReconnect()
        }

        ws.onerror = () => {
            // onclose will fire after this, which handles reconnect
            ws?.close()
        }
    }

    function scheduleReconnect() {
        if (reconnectTimer) return
        reconnectTimer = setTimeout(() => {
            reconnectTimer = null
            connect()
        }, RECONNECT_DELAY_MS)
    }

    connect()

    // Return a disconnect function
    return () => {
        if (reconnectTimer) {
            clearTimeout(reconnectTimer)
            reconnectTimer = null
        }
        ws?.close()
    }
}
