import { ON_NEW_RAW_OCCURENCE } from "../types/common/ErisEvents"

const DEFAULT_PORT = 9222
const RECONNECT_DELAY_MS = 3000

export function connectToDebugServer(port: number = DEFAULT_PORT) {
    let ws: WebSocket | null = null
    let reconnectTimer: ReturnType<typeof setTimeout> | null = null

    function connect() {
        ws = new WebSocket(`ws://127.0.0.1:${port}`)

        ws.onopen = () => {
            console.log(`[Eris] Connected to debug server on port ${port}`)
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
