import {
    FASTElement,
    attr,
    css,
    customElement,
    html,
    observable,
    repeat,
    when,
} from "@microsoft/fast-element"
import windcss from "../../../components/common/styles/wind.css?raw"
import { getObjectContents } from "../occurences/matter-occurence-templates/common-occurence"

export type CausalityTreeNode = {
    guid: string
    typeName: string
    description: string
    content: any
    children: CausalityTreeNode[]
    alreadyShown: boolean
    missing: boolean
}

const styles = css`
    ${windcss}

    :host {
        display: block;
        font-family: "Fell Pica";
        font-size: 0.8rem;
    }

    .row {
        display: flex;
        flex-direction: row;
        align-items: baseline;
        gap: 0.4rem;
        padding: 0.1rem 0;
    }

    .toggle {
        background: transparent;
        border: none;
        color: inherit;
        cursor: pointer;
        width: 1rem;
        text-align: center;
        font-family: inherit;
        font-size: inherit;
        padding: 0;
    }

    .toggle.leaf {
        visibility: hidden;
    }

    .type {
        color: #ffe300;
    }

    .type.clickable {
        cursor: pointer;
        text-decoration: underline dotted transparent;
        transition: text-decoration-color 0.15s;
    }

    .type.clickable:hover {
        text-decoration-color: #ffe300;
    }

    .guid {
        color: #777;
        font-size: 0.7rem;
    }

    /* margin-left matches .children so the details border-left is a
       continuous extension of the children indentation line (both at
       0.5rem under the toggle). padding-left then pushes the actual
       content inward for readability. */
    .details {
        margin-left: 0.5rem;
        padding: 0.3rem 0.6rem 0.3rem 1.1rem;
        border-left: 1px dashed #444;
        background-color: #00000033;
        color: aliceblue;
    }

    .details .full-guid {
        color: #777;
        font-size: 0.7rem;
        font-family: monospace;
        word-break: break-all;
    }

    .details .description {
        color: antiquewhite;
        margin: 0.2rem 0;
    }

    .details ul {
        list-style-type: square;
        color: #b5d4c8;
        padding-inline-start: 1.2rem;
        margin: 0.2rem 0;
    }

    .details ul ul {
        list-style-type: circle;
    }

    .already-shown {
        color: #ff8c00;
    }

    .missing {
        color: #ff4040;
    }

    /* The border-left lives at margin-left from the parent's left edge.
       Toggle button is 1rem wide and the +/− character is centered, so
       centering the line at 0.5rem makes it appear directly under the
       toggle. padding-left then pushes child content to the same column
       as the parent's type name (toggle 1rem + row-gap 0.4rem = 1.4rem). */
    .children {
        margin-left: 0.5rem;
        border-left: 1px dashed #444;
        padding-left: 0.9rem;
    }
`

const template = html<CausalityNode>`
    <div class="row"
        @mouseenter="${x => x.hoverEnter()}"
        @mouseleave="${x => x.hoverLeave()}">
        <button
            class="toggle ${x => (x.node.children.length === 0 ? "leaf" : "")}"
            @click="${x => x.toggle()}">
            ${x => (x.node.children.length === 0 ? "·" : x.collapsed ? "+" : "−")}
        </button>
        ${when(
            x => x.node.alreadyShown,
            html<CausalityNode>`
                <span class="already-shown">↑ shown above</span>
                <span class="type">${x => x.node.typeName}</span>
                <span class="guid">${x => shortGuid(x.node.guid)}</span>
            `,
        )}
        ${when(
            x => x.node.missing,
            html<CausalityNode>`
                <span class="missing">⚠ missing</span>
                <span class="guid">${x => shortGuid(x.node.guid)}</span>
            `,
        )}
        ${when(
            x => !x.node.alreadyShown && !x.node.missing,
            html<CausalityNode>`
                <span
                    class="type clickable"
                    @click="${x => x.toggleDetails()}"
                    >${x => x.node.typeName}</span>
                <span class="guid">${x => shortGuid(x.node.guid)}</span>
            `,
        )}
    </div>
    ${when(
        x => x.detailsOpen && !x.node.alreadyShown && !x.node.missing,
        html<CausalityNode>`
            <div class="details">
                <div class="full-guid">${x => x.node.guid}</div>
                ${when(
                    x => x.node.description && x.node.description.length > 0,
                    html<CausalityNode>`
                        <div class="description">${x => x.node.description}</div>
                    `,
                )}
                <ul :innerHTML="${x => getObjectContents(x.node.content ?? {}, "", 0)}"></ul>
            </div>
        `,
    )}
    ${when(
        x => !x.collapsed && x.node.children.length > 0,
        html<CausalityNode>`
            <div class="children">
                ${repeat(
                    x => x.node.children,
                    html<CausalityTreeNode>`
                        <causality-node :node="${x => x}"></causality-node>
                    `,
                )}
            </div>
        `,
    )}
`

@customElement({
    name: "causality-node",
    template,
    styles,
})
export class CausalityNode extends FASTElement {
    @observable node: CausalityTreeNode
    @attr({ mode: "boolean" }) collapsed: boolean = true
    @attr({ mode: "boolean" }) detailsOpen: boolean = false

    private getGefilde() {
        return document.getElementById("eris-realm-events") as any
    }

    hoverEnter() {
        if (this.node.missing) return
        this.getGefilde()?.hoveredMatterGuid$?.next(this.node.guid)
    }

    hoverLeave() {
        this.getGefilde()?.hoveredMatterGuid$?.next(null)
    }

    toggle() {
        if (!this.collapsed) {
            cascadeResetDescendants(this)
        }
        this.collapsed = !this.collapsed
    }

    toggleDetails() {
        this.detailsOpen = !this.detailsOpen
    }
}

// When a node is collapsed, walk its visible descendants and reset their
// details/collapse state. Without this, re-expanding restores whatever
// sprawl was there before — which defeats the point of collapsing.
// Recurses because querySelectorAll does not pierce nested shadow roots.
function cascadeResetDescendants(node: CausalityNode) {
    const descendants = node.shadowRoot?.querySelectorAll("causality-node")
    descendants?.forEach(d => {
        const child = d as CausalityNode
        cascadeResetDescendants(child)
        child.detailsOpen = false
        child.collapsed = true
    })
}

function shortGuid(guid: string): string {
    return guid.slice(0, 8)
}
