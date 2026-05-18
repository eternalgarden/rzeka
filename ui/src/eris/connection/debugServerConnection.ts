import {
    ConnectionStatus,
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
                const occurence = JSON.parse(event.data)

                const customEvent = new CustomEvent(ON_NEW_RAW_OCCURENCE, {
                    detail: occurence,
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
