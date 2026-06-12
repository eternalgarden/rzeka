import {
    FASTElement,
    css,
    customElement,
    html,
    observable,
    when,
} from "@microsoft/fast-element"
import { ON_RIVER_NAME_CHANGE } from "../../types/common/ErisEvents"
import { getRiverName } from "../../connection/debugServerConnection"

const styles = css`
    :host {
        display: inline-flex;
        align-items: center;
        font-family: "avara-bold";
        font-size: 0.95rem;
        padding: 0.2rem 0.6rem;
        color: ghostwhite;
        opacity: 0.85;
    }

    .prefix {
        color: #888;
        margin-right: 0.4rem;
    }

    .name {
        color: #9ecbff;
    }

    .placeholder {
        color: #555;
        font-style: italic;
    }
`

const template = html<RiverNameElement>`
    ${when(
        x => x.riverName !== null,
        html<RiverNameElement>`
            <span class="prefix">river:</span>
            <span class="name">${x => x.riverName}</span>
        `,
    )}
    ${when(
        x => x.riverName === null,
        html<RiverNameElement>`<span class="placeholder">no river</span>`,
    )}
`

@customElement({
    name: "eris-river-name",
    template,
    styles,
})
export class RiverNameElement extends FASTElement {
    @observable riverName: string | null = null

    private readonly handler = (e: Event) => {
        this.riverName = (e as CustomEvent<string | null>).detail
    }

    connectedCallback(): void {
        super.connectedCallback()
        document.addEventListener(ON_RIVER_NAME_CHANGE, this.handler)
        this.riverName = getRiverName()
    }

    disconnectedCallback(): void {
        document.removeEventListener(ON_RIVER_NAME_CHANGE, this.handler)
        super.disconnectedCallback()
    }
}
