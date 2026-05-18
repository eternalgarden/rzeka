import {
    FASTElement,
    css,
    customElement,
    html,
    observable,
    when,
} from "@microsoft/fast-element"
import {
    ConnectionStatus as Status,
    ON_CONNECTION_STATUS_CHANGE,
} from "../../types/common/ErisEvents"
import { getConnectionStatus } from "../../connection/debugServerConnection"

const styles = css`
    :host {
        display: inline-flex;
        align-items: center;
        gap: 0.4rem;
        font-family: monospace;
        font-size: 0.85rem;
        padding: 0.2rem 0.5rem;
    }

    .dot {
        width: 0.55rem;
        height: 0.55rem;
        border-radius: 50%;
        display: inline-block;
    }

    .connected .dot {
        background-color: #4ade80;
        box-shadow: 0 0 4px #4ade80;
    }

    .connected .label {
        color: #4ade80;
    }

    .disconnected .dot {
        background-color: #ff4d6c;
        box-shadow: 0 0 4px #ff4d6c;
    }

    .disconnected .label {
        color: #ff4d6c;
    }

    .retry {
        color: #888;
        font-size: 0.8em;
    }
`

const template = html<ConnectionStatusElement>`
    <div class="${x => (x.status === "connected" ? "connected" : "disconnected")}">
        <span class="dot"></span>
        <span class="label">
            Status: ${x => (x.status === "connected" ? "Connected" : "Disconnected")}
        </span>
        ${when(
            x => x.status === "disconnected" && x.retryInSeconds !== null,
            html<ConnectionStatusElement>`
                <span class="retry">- retry in ${x => x.retryInSeconds}s</span>
            `,
        )}
    </div>
`

@customElement({
    name: "eris-connection-status",
    template,
    styles,
})
export class ConnectionStatusElement extends FASTElement {
    @observable status: "connected" | "disconnected" = "disconnected"
    @observable retryInSeconds: number | null = null

    private reconnectAt: number | null = null
    private tickInterval: ReturnType<typeof setInterval> | null = null

    private readonly handler = (e: Event) => {
        this.applyStatus((e as CustomEvent<Status>).detail)
    }

    connectedCallback(): void {
        super.connectedCallback()
        document.addEventListener(ON_CONNECTION_STATUS_CHANGE, this.handler)
        this.applyStatus(getConnectionStatus())
    }

    disconnectedCallback(): void {
        document.removeEventListener(ON_CONNECTION_STATUS_CHANGE, this.handler)
        this.stopTick()
        super.disconnectedCallback()
    }

    private applyStatus(detail: Status) {
        if (detail.status === "connected") {
            this.status = "connected"
            this.reconnectAt = null
            this.retryInSeconds = null
            this.stopTick()
            return
        }

        this.status = "disconnected"
        this.reconnectAt = detail.reconnectAt
        this.updateRetrySeconds()

        if (detail.reconnectAt !== null) {
            this.startTick()
        } else {
            this.stopTick()
        }
    }

    private startTick() {
        if (this.tickInterval !== null) return
        this.tickInterval = setInterval(() => this.updateRetrySeconds(), 500)
    }

    private stopTick() {
        if (this.tickInterval === null) return
        clearInterval(this.tickInterval)
        this.tickInterval = null
    }

    private updateRetrySeconds() {
        if (this.reconnectAt === null) {
            this.retryInSeconds = null
            return
        }
        const remaining = Math.max(
            0,
            Math.ceil((this.reconnectAt - Date.now()) / 1000),
        )
        this.retryInSeconds = remaining
    }
}
