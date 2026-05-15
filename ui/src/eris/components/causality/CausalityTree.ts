import {
    FASTElement,
    css,
    customElement,
    html,
    observable,
    when,
} from "@microsoft/fast-element"
import { Subscription } from "rxjs"
import { GefildeDesVorkommen } from "../GefildeDesVorkommen"
import { MatterArchive } from "../../types/archives/MatterArchive"
import { CausalityNode, CausalityTreeNode } from "./CausalityNode"
import "./CausalityNode"
import windcss from "../../../components/common/styles/wind.css?raw"

const styles = css`
    ${windcss}

    :host {
        display: flex;
        flex-direction: column;
        height: 100%;
        color: aliceblue;
        font-family: "Fell Pica";
        background-color: #17131385;
    }

    .header {
        padding: 0.4rem 0.6rem;
        font-family: monospace;
        color: #00ffc6;
        border-bottom: 1px dashed #444;
    }

    .body {
        flex: 1 1 auto;
        overflow-y: auto;
        padding: 0.4rem 0.6rem;
    }

    .empty {
        color: #888;
        font-style: italic;
    }
`

const template = html<CausalityTree>`
    <div class="header">🧶 causality tree</div>
    <div class="body">
        ${when(
            x => x.rootNode === null,
            html<CausalityTree>`
                <div class="empty">📍 pin a matter to see its causality.</div>
            `,
        )}
        ${when(
            x => x.rootNode !== null,
            html<CausalityTree>`
                <causality-node
                    :node="${x => x.rootNode}"
                    :collapsed="${_ => false}"></causality-node>
            `,
        )}
    </div>
`

@customElement({
    name: "eris-causality-tree",
    template,
    styles,
})
export class CausalityTree extends FASTElement {
    @observable rootNode: CausalityTreeNode | null = null

    private archive: MatterArchive | undefined
    private selectionSubscription: Subscription | undefined

    connectedCallback(): void {
        super.connectedCallback()

        const gefilde = document.getElementById(
            "eris-realm-events",
        ) as GefildeDesVorkommen | null

        if (gefilde === null) {
            console.error("CausalityTree: <eris-realm-events> not found in DOM")
            return
        }

        this.archive = gefilde.occurenceArchive.matterArchive

        this.selectionSubscription = gefilde.selectedMatterGuid$.subscribe(
            guid => {
                this.rootNode =
                    guid === null || this.archive === undefined
                        ? null
                        : buildCausalityTree(guid, this.archive)
            },
        )
    }

    disconnectedCallback(): void {
        this.selectionSubscription?.unsubscribe()
        super.disconnectedCallback()
    }
}

// Walks ancestors of `rootGuid` and produces a spanning tree of the causal DAG.
// `seen` is mutated and shared across the whole walk so each matter is
// rendered exactly once — repeat encounters become `alreadyShown` leaves.
// Without this, state matter like DataRootPathState that fans into hundreds
// of descendants would re-expand its full subtree under every consumer.
function buildCausalityTree(
    rootGuid: string,
    archive: MatterArchive,
    seen: Set<string> = new Set(),
): CausalityTreeNode {
    if (!archive.hasMatter(rootGuid)) {
        return {
            guid: rootGuid,
            typeName: "",
            description: "",
            content: null,
            children: [],
            alreadyShown: false,
            missing: true,
        }
    }

    const type = archive.getMatterType(rootGuid)
    const data = archive.getMatterData(rootGuid)

    if (seen.has(rootGuid)) {
        return {
            guid: rootGuid,
            typeName: type.name,
            description: data.description,
            content: data.content,
            children: [],
            alreadyShown: true,
            missing: false,
        }
    }
    seen.add(rootGuid)

    const children = data.circumstances.map(c =>
        buildCausalityTree(c, archive, seen),
    )

    return {
        guid: rootGuid,
        typeName: type.name,
        description: data.description,
        content: data.content,
        children,
        alreadyShown: false,
        missing: false,
    }
}
