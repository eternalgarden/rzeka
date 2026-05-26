import { attr } from "@microsoft/fast-element"
import {
    FASTElement,
    css,
    customElement,
    html,
    observable,
} from "@microsoft/fast-element"
import { MatterOccurenceCategory } from "../../types/occurence-categories/MatterOccurenceCategory"
import { GefildeDesVorkommen } from "../GefildeDesVorkommen"
import { Subscription } from "rxjs"
import { MatterData } from "../../types/common/matter/MatterData"
import { Type } from "../../types/common/Type"
import {
    ArchivedMatterOccurence,
    ArchivedReceivedMatterOccurence,
} from "../../types/occurences-archived/ArchivedMatterOccurence"
import { SpellArchive } from "../../types/archives/SpellArchive"
import { MatterArchive } from "../../types/archives/MatterArchive"
import { ShapedMatterOccurenceTemplate } from "./matter-occurence-templates/shaped-matter-template"
import { ReceivedMatterOccurenceTemplate } from "./matter-occurence-templates/received-matter-template"

/* 🎨 styles */
import windcss from "../../../components/common/styles/wind.css?raw"
import matterStyles from "./styles/matter.css?raw"


const styles = css`
    ${windcss}
    ${matterStyles}
`

const matterOccurenceTemplates = {
    Shaped: ShapedMatterOccurenceTemplate,
    Received: ReceivedMatterOccurenceTemplate,
    Error: html`<h1>🐺🌄 I think this is not used.</h1>`,
}

/* MATTER ITEM TEMPLATE */
const template = html<MatterItem>`
    ${x => matterOccurenceTemplates[x.matterOccurenceCategory]}
`

@customElement({
    name: "matter-item",
    template,
    styles,
})
export class MatterItem extends FASTElement {
    // 🌲 Attributes
    @attr gefilde: GefildeDesVorkommen
    @attr occurence: ArchivedMatterOccurence
    @attr({ mode: "boolean" }) selected: boolean = false
    @attr({ mode: "boolean" }) hovered: boolean = false

    // ☔ Derived observables - populated by initFromOccurence(), not set directly.
    // These are separate @observable fields (rather than template expressions reading
    // off occurence directly) because some of them require archive lookups and one
    // (listOfReceivingSpells) is fed by an RxJS subscription, not a simple property read.
    @observable matterOccurenceCategory: MatterOccurenceCategory
    @observable timestamp: number
    @observable matterType: Type
    @observable matterData: MatterData
    @observable matterGuid: string
    @observable matterContent: any
    /* 🐺🗃️🗝️ Two shady 'situational' fields */
    /* Clean solution would be to split MatterItem into two separate Elements */
    /* But it is still quite readable and there won't be more matter occurence types. */
    @observable spellGuid: string = "" // Only Shaped
    @observable listOfReceivingSpells: string[] = [] // Only Received

    spellArchive: SpellArchive
    matterArchive: MatterArchive

    receivedSubscription: Subscription | undefined
    selectionSubscription: Subscription
    hoverSubscription: Subscription
    wildcardSubscription: Subscription

    connectedCallback(): void {
        this.spellArchive = this.gefilde.occurenceArchive.spellsArchive
        this.matterArchive = this.gefilde.occurenceArchive.matterArchive
        this.initFromOccurence(this.occurence)
        super.connectedCallback()
    }

    // FAST calls ${propertyName}Changed(old, new) whenever a decorated property is assigned.
    // This fires when FAST's repeat directive reuses this DOM element for a different occurrence
    // (which happens on every prepend to listedOccurences - all existing elements shift by one
    // index and get new occurence values assigned). Without this, the derived @observable fields
    // above would stay stale from the previous occurrence while the template also reads the new
    // occurence directly, producing a split display (correct guid, wrong type name, wrong who).
    // Guard: FAST can assign occurence before connectedCallback runs; in that case matterArchive
    // is not set yet and connectedCallback will call initFromOccurence on its own.
    occurenceChanged(_old: ArchivedMatterOccurence, newVal: ArchivedMatterOccurence): void {
        if (!this.matterArchive) return

        this.wildcardSubscription?.unsubscribe()
        this.wildcardSubscription = undefined
        this.selectionSubscription?.unsubscribe()
        this.hoverSubscription?.unsubscribe()

        this.initFromOccurence(newVal)
    }

    // Single source of truth for deriving observable state from an occurrence.
    // Called by both connectedCallback (initial render) and occurenceChanged (element reuse).
    // If you add a new @observable field that depends on occurence, add it here - not in
    // connectedCallback - otherwise it will go stale on element reuse.
    private initFromOccurence(occ: ArchivedMatterOccurence): void {
        this.matterGuid = occ.matterGuid
        this.matterOccurenceCategory = occ.matterOccurenceCategory
        this.timestamp = occ.timestamp
        this.matterType = this.matterArchive.getMatterType(this.matterGuid)
        this.matterData = this.matterArchive.getMatterData(this.matterGuid)
        this.matterContent = this.matterData.content
        this.listOfReceivingSpells = []

        if (this.matterOccurenceCategory === MatterOccurenceCategory.Received) {
            // newReceiverObservable is a BehaviorSubject, so this emits the current
            // receiver list immediately and then again whenever a new spell receives
            // the same matter instance.
            this.wildcardSubscription = this.getReceivalSubscription()
        }

        this.selectionSubscription = this.gefilde.selectedMatterGuid$.subscribe(
            guid => (this.selected = guid === this.matterGuid),
        )

        // Only shaped occurrences can appear as nodes in the causality tree,
        // so only they need the hover highlight.
        this.hoverSubscription = this.gefilde.hoveredMatterGuid$.subscribe(
            guid => (this.hovered =
                this.matterOccurenceCategory === MatterOccurenceCategory.Shaped &&
                guid === this.matterGuid),
        )
    }

    disconnectedCallback(): void {
        this.receivedSubscription?.unsubscribe()
        this.wildcardSubscription?.unsubscribe()
        this.selectionSubscription?.unsubscribe()
        this.hoverSubscription?.unsubscribe()

        super.disconnectedCallback()
    }

    selectThisMatter(event: Event) {
        event.stopPropagation()
        const subject = this.gefilde.selectedMatterGuid$
        subject.next(subject.value === this.matterGuid ? null : this.matterGuid)
    }

    hasShapedMatterCircumstances() {
        if (this.matterOccurenceCategory == MatterOccurenceCategory.Shaped)
            return this.matterData.circumstances.length == 0
        else return false
    }

    getReceivalSubscription(): Subscription {
        const receivedMattterOccurence = this
            .occurence as ArchivedReceivedMatterOccurence

        return receivedMattterOccurence.newReceiverObservable.subscribe(
            receivers => {
                this.listOfReceivingSpells = receivers
            }
        )
    }
}
