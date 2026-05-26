import {
    FASTElement,
    ViewTemplate,
    customElement,
    html,
    css,
    observable,
    repeat,
    when,
    ref,
} from "@microsoft/fast-element"

import { OccurenceCategory } from "../types/occurence-categories/OccurenceCategory"
import { BehaviorSubject, Subscription, fromEvent } from "rxjs"
import { map } from "rxjs/operators"
import { IArchivedOccurence } from "../types/occurences-archived/IArchivedOccurence"
import { IArchivedMatterOccurence } from "../types/occurences-archived/IArchivedMatterOccurence"
import {
    ArchivedReceivedMatterOccurence,
    ArchivedShapedMatterOccurence,
} from "../types/occurences-archived/ArchivedMatterOccurence"
import { ArchivedMessageOccurence } from "../types/occurences-archived/ArchivedMessageOccurence"
import { MessageType } from "../types/common/message/MessageTypeEnum"
import { MatterOccurenceCategory } from "../types/occurence-categories/MatterOccurenceCategory"
import { OccurencesArchive } from "../types/archives/OccurencesArchive"

type WhoDescriptionOption = { value: string; label: string }
import {
    ON_CLEAR_OCCURENCES,
    ON_SET_FOCUS_TO_SEARCHBAR,
} from "../types/common/ErisEvents"

const STYLES = css`
    :host {
        contain: content;
    }

    .region {
        background-color: #343c3c2b;
        color: aliceblue;
    }

    .header {
        font-family: monospace;
    }

    .menu {
        position: sticky;
        top: 0;
        z-index: 1;
        width: 100%;
        display: flex;
        flex-direction: column;
        background-color: #17131385;
        backdrop-filter: sepia(90%);
        backdrop-filter: blur(2px);
        padding: 0.3rem;
        font-family: monospace;
    }

    .header-row {
        display: flex;
        align-items: center;
        gap: 0.5rem;
        flex-wrap: wrap;
    }

    .msg-badge {
        display: inline-flex;
        align-items: center;
        gap: 0.25rem;
        padding: 0.1rem 0.4rem;
        border-radius: 3px;
        font-family: monospace;
        font-size: 0.85rem;
        cursor: pointer;
        transition: background-color 0.15s;
        border: 1px solid transparent;
    }

    .msg-badge.Horror {
        background-color: #5a1a1a;
        border-color: #ff6b6b88;
        color: #ff9999;
    }
    .msg-badge.Horror:hover { background-color: #7a2222; }
    .msg-badge.Horror.active {
        background-color: #8b0000;
        border-color: #ff6b6b;
        color: #ffcccc;
    }

    .msg-badge.Hunch {
        background-color: #3a3010;
        border-color: #ffdd6688;
        color: #ffdd99;
    }
    .msg-badge.Hunch:hover { background-color: #504218; }
    .msg-badge.Hunch.active {
        background-color: #5a4a00;
        border-color: #ffdd66;
        color: #fff0bb;
    }

    .msg-badge.Hint {
        background-color: #0e2040;
        border-color: #6699ff88;
        color: #99bbff;
    }
    .msg-badge.Hint:hover { background-color: #1a3060; }
    .msg-badge.Hint.active {
        background-color: #0a1e5e;
        border-color: #6699ff;
        color: #ccdeff;
    }

    .who-filter {
        display: flex;
        gap: 0.3rem;
        margin-top: 0.2rem;
    }

    input.searchbar {
        height: 22px;
        background-color: transparent;
        color: antiquewhite;
        border: none;
        outline: none;
    }

    /* https://www.fast.design/docs/fast-element/leveraging-css */
    :host([hidden]) {
        display: none;
    }

    /* why doesnt that work? */
    .searchbar:focus-visible {
        border: none;
    }

    select {
        outline: none;
        border: none;
        background-color: #1e2a2a;
        color: antiquewhite;
        font-family: monospace;
        font-size: 0.8rem;
        max-width: 12rem;
        cursor: pointer;
    }

    option {
        background-color: #1e2a2a;
    }
`

const occurenceTemplates = {
    Spell: html`
        <spell-item
            :gefilde=${(_, c) => c.parent as GefildeDesVorkommen}
            :spellOccurence=${x => x}></spell-item>
    `,
    Matter: html`
        <matter-item
            :gefilde=${(_, c) => c.parent as GefildeDesVorkommen}
            :occurence=${x => {
                // * so this is actually really interesting and kindof wild
                // * how fast manages to slot in that conte...xt?
                // * notice it is getting provided by the repeat method below in parent template
                // console.log(x);
                return x
            }} />
    `,
    Message: html` <message-item
        :gefilde=${(_, c) => c.parent as GefildeDesVorkommen}
        :occurence=${x => {
            // * so this is actually really interesting and kindof wild
            // * how fast manages to slot in that conte...xt?
            // * notice it is getting provided by the repeat method below in parent template
            // console.log(x);
            return x
        }} />`,
}

function messageBadgeEmoji(type: MessageType): string {
    if (type === MessageType.Horror) return "🧨"
    if (type === MessageType.Hunch) return "⚠️"
    return "🐇"
}

// Designating type with ': ViewTemplate<OccurencesContainerElement>' is not necessary
// it's a 'typescript' thing, here for clarity,
// its probably a good practice but honestly it does a bit occlude the code
const LITTLE_TEMPLATE: ViewTemplate<GefildeDesVorkommen> = html<GefildeDesVorkommen>`
    <div class="menu">
        <div class="header-row">
            <div class="header">😼🗝️🗃️ occurences</div>
            <button
                class="msg-badge Hint"
                title="Dump listedOccurences to console as JSON"
                @click="${x => x.dumpListedOccurences()}">🗃️ dump</button>
            ${repeat(
                x => x.messageBadges,
                html<{ type: MessageType; count: number; active: boolean }>`
                    <button
                        class="msg-badge ${x => x.type} ${x => (x.active ? "active" : "")}"
                        @click="${(x, c) =>
                            (c.parent as GefildeDesVorkommen).toggleMessageTypeFilter(x.type)}">
                        ${x => messageBadgeEmoji(x.type)} ${x => x.count}
                    </button>
                `
            )}
        </div>
        <div class="who-filter">
            <select ${ref("whoTypeSelect")}>
                <option value="">all owners</option>
                ${repeat(
                    x => x.whoTypeOptions,
                    html<string>`<option value="${x => x}">${x => x}</option>`
                )}
            </select>
            ${when(
                x => !!x.whoTypeFilter,
                html<GefildeDesVorkommen>`
                    <select
                        ${ref("whoDescriptionSelect")}
                        @change="${(x, c) => x.onDescriptionSelectChange(c.event)}">
                        <option value="">all instances</option>
                        ${repeat(
                            x => x.whoDescriptionOptions,
                            html<WhoDescriptionOption>`
                                <option value="${x => x.value}">${x => x.label}</option>
                            `
                        )}
                    </select>
                `
            )}
        </div>
        <input
            class="searchbar"
            ${ref("searchbar")}
            type="text"
            tabindex="0"
            placeholder="filter by type..." />
    </div>
    <div class="occurences">
        ${repeat(
            x => x.listedOccurences,
            html<IArchivedOccurence>`
                ${x => occurenceTemplates[x.occurenceCategory]}
            `,
            { positioning: true }
        )}
    </div>
`

@customElement({
    name: "eris-realm-events",
    template: LITTLE_TEMPLATE,
    styles: STYLES,
})
export class GefildeDesVorkommen extends FASTElement {
    // 🪞 Template html references
    searchbar: HTMLInputElement
    whoTypeSelect: HTMLSelectElement
    whoDescriptionSelect: HTMLSelectElement

    // 🕊️ Displayed occurences
    @observable listedOccurences: IArchivedOccurence[] = []
    private allOccurences: IArchivedOccurence[] = []
    private searchTerm: string = ""

    // 💎 Who filter
    @observable whoTypeOptions: string[] = []
    @observable whoDescriptionOptions: WhoDescriptionOption[] = []
    @observable whoTypeFilter: string = ""
    private whoDescriptionFilter: string = ""

    // 🗣️ Message type badges — counts + active filter
    private messageCounts: Record<MessageType, number> = {
        [MessageType.Horror]: 0,
        [MessageType.Hunch]: 0,
        [MessageType.Hint]: 0,
    }
    private messageTypeFilter: MessageType | null = null
    @observable messageBadges: { type: MessageType; count: number; active: boolean }[] = []

    // 🦄 Purposefully public
    occurenceArchive: OccurencesArchive

    // 🎯 Currently selected matter guid — drives the causality tree window.
    // Null means nothing is selected.
    readonly selectedMatterGuid$ = new BehaviorSubject<string | null>(null)

    // 🟢 Currently hovered matter guid from the causality tree — drives the
    // green highlight on the corresponding shaped occurrence in the list.
    // Null means nothing is hovered.
    readonly hoveredMatterGuid$ = new BehaviorSubject<string | null>(null)

    private lifecycleSubscriptions: Subscription[] = []

    constructor() {
        super()

        // ! Important ::: Hard-set matter memory limit
        // Currently as a simple constant, possiblly in future to be passed
        // as a parameter from Sanctuary
        const occurenceMemoryLimit = 5000

        this.occurenceArchive = new OccurencesArchive(occurenceMemoryLimit)

        this.registerOccurenceArchiveListeners()
    }

    connectedCallback(): void {
        super.connectedCallback()

        this.registerFilterListeners()

        this.lifecycleSubscriptions.push(
            fromEvent(document, ON_SET_FOCUS_TO_SEARCHBAR).subscribe(
                _ => this.setFocusToSearchbar()
            )
        )

        this.lifecycleSubscriptions.push(
            fromEvent(document, ON_CLEAR_OCCURENCES).subscribe(
                _ => this.clearOccurences()
            )
        )
    }

    disconnectedCallback() {
        for (let i = 0; i < this.lifecycleSubscriptions.length; i++) {
            const element = this.lifecycleSubscriptions[i]
            element.unsubscribe()
        }

        super.disconnectedCallback()
    }

    private registerFilterListeners() {
        this.lifecycleSubscriptions.push(
            fromEvent(this.searchbar, "input").pipe(
                map(x => (x.target as HTMLInputElement).value.toLowerCase())
            ).subscribe(term => {
                this.searchTerm = term
                this.applyFilter()
            })
        )

        this.lifecycleSubscriptions.push(
            fromEvent(this.whoTypeSelect, "change").pipe(
                map(e => (e.target as HTMLSelectElement).value)
            ).subscribe(val => {
                this.whoTypeFilter = val
                this.whoDescriptionFilter = ""
                this.refreshWhoDescriptionOptions()
                this.applyFilter()
            })
        )

        // Refresh type list whenever a new spell is registered
        this.lifecycleSubscriptions.push(
            this.occurenceArchive.spellsArchive.newSpellOccurenceProcessed.subscribe(() => {
                this.refreshWhoTypeOptions()
                if (this.whoTypeFilter) this.refreshWhoDescriptionOptions()
            })
        )
    }

    onDescriptionSelectChange(event: Event) {
        this.whoDescriptionFilter = (event.target as HTMLSelectElement).value
        this.applyFilter()
    }

    private refreshWhoTypeOptions() {
        this.whoTypeOptions = this.occurenceArchive.spellsArchive.getDistinctWhoTypes()
    }

    private refreshWhoDescriptionOptions() {
        const descs = this.occurenceArchive.spellsArchive.getWhoDescriptionsForType(this.whoTypeFilter)
        this.whoDescriptionOptions = descs.map(d =>
            d === null
                ? { value: "__unnamed__", label: "unnamed" }
                : { value: d, label: d }
        )
    }

    toggleMessageTypeFilter(type: MessageType) {
        this.messageTypeFilter = this.messageTypeFilter === type ? null : type
        this.refreshMessageBadges()
        this.applyFilter()
    }

    private refreshMessageBadges() {
        this.messageBadges = (Object.keys(this.messageCounts) as MessageType[])
            .filter(type => this.messageCounts[type] > 0)
            .map(type => ({
                type,
                count: this.messageCounts[type],
                active: this.messageTypeFilter === type,
            }))
    }

    private applyFilter() {
        this.listedOccurences = this.allOccurences.filter(occ =>
            this.matchesMessageTypeFilter(occ) && this.matchesSearch(occ)
        )
    }

    private matchesMessageTypeFilter(occ: IArchivedOccurence): boolean {
        if (!this.messageTypeFilter) return true
        if (occ.occurenceCategory !== OccurenceCategory.Message) return false
        return (occ as ArchivedMessageOccurence).messageType === this.messageTypeFilter
    }

    private matchesSearch(occ: IArchivedOccurence): boolean {
        if (!this.matchesWhoFilter(occ)) return false
        if (occ.occurenceCategory !== OccurenceCategory.Matter) return true
        if (!this.searchTerm) return true
        const matterOcc = occ as IArchivedMatterOccurence
        const archive = this.occurenceArchive.matterArchive
        if (!archive.hasMatter(matterOcc.matterGuid)) return true
        return archive
            .getMatterType(matterOcc.matterGuid)
            .name.toLowerCase()
            .includes(this.searchTerm)
    }

    private matchesWhoFilter(occ: IArchivedOccurence): boolean {
        if (!this.whoTypeFilter) return true
        if (occ.occurenceCategory !== OccurenceCategory.Matter) return true

        const matterOcc = occ as IArchivedMatterOccurence
        const spellGuid = this.occurenceArchive.matterArchive.getMatterShapingSpellGuid(
            matterOcc.matterGuid
        )
        if (!spellGuid) return true

        const who = this.occurenceArchive.spellsArchive.getSpellWho(spellGuid)
        if (!who) return true

        if (who.WhosType.name !== this.whoTypeFilter) return false
        if (!this.whoDescriptionFilter) return true
        if (this.whoDescriptionFilter === "__unnamed__") return !who.WhosDescription
        return who.WhosDescription === this.whoDescriptionFilter
    }

    private registerOccurenceArchiveListeners() {
        const sub: Subscription = this.occurenceArchive.newProcessedOccurence
            .subscribe(occ => {
                if (
                    occ.occurenceCategory !== OccurenceCategory.Matter &&
                    occ.occurenceCategory !== OccurenceCategory.Message
                )
                    return

                if (occ.occurenceCategory === OccurenceCategory.Message) {
                    const msgType = (occ as ArchivedMessageOccurence).messageType
                    this.messageCounts[msgType]++
                    this.refreshMessageBadges()
                }

                this.allOccurences.unshift(occ)
                if (this.matchesMessageTypeFilter(occ) && this.matchesSearch(occ))
                    this.listedOccurences = [occ, ...this.listedOccurences]
            })

        this.lifecycleSubscriptions.push(sub)
    }

    dumpListedOccurences() {
        const spells = this.occurenceArchive.spellsArchive
        const matters = this.occurenceArchive.matterArchive

        const serialized = this.listedOccurences.map((occ, idx) => {
            const base = {
                idx,
                occurenceCategory: occ.occurenceCategory,
                guid: occ.guid,
                timestamp: occ.timestamp,
            }

            if (occ.occurenceCategory === OccurenceCategory.Matter) {
                const m = occ as IArchivedMatterOccurence
                const matterType = matters.hasMatter(m.matterGuid)
                    ? matters.getMatterType(m.matterGuid).name
                    : "UNKNOWN"

                if (m.matterOccurenceCategory === MatterOccurenceCategory.Shaped) {
                    const shaped = occ as ArchivedShapedMatterOccurence
                    return {
                        ...base,
                        matterOccurenceCategory: "Shaped",
                        matterGuid: m.matterGuid,
                        matterType,
                        shapedBySpell: shaped.spellGuid,
                        shapedByCaster: spells.getSpellCasterName(shaped.spellGuid),
                        shapedByTitle: spells.getSpellTitle(shaped.spellGuid),
                    }
                }

                if (m.matterOccurenceCategory === MatterOccurenceCategory.Received) {
                    const received = occ as ArchivedReceivedMatterOccurence
                    // Access the private list via the subject's current value
                    const receiverSubjectValue = (received as any).newReceiverSubject?.getValue?.() ?? []
                    return {
                        ...base,
                        matterOccurenceCategory: "Received",
                        matterGuid: m.matterGuid,
                        matterType,
                        receivedBySpells: receiverSubjectValue.map((sg: string) => ({
                            spellGuid: sg,
                            caster: spells.getSpellCasterName(sg),
                            title: spells.getSpellTitle(sg),
                        })),
                    }
                }

                return { ...base, matterOccurenceCategory: m.matterOccurenceCategory, matterGuid: m.matterGuid, matterType }
            }

            if (occ.occurenceCategory === OccurenceCategory.Message) {
                const msg = occ as ArchivedMessageOccurence
                return { ...base, messageType: msg.messageType, message: msg.message }
            }

            return base
        })

        console.log("=== listedOccurences dump ===")
        console.log(JSON.stringify(serialized, null, 2))
    }

    setFocusToSearchbar() {
        this.searchbar.focus()
    }

    clearOccurences() {
        this.allOccurences = []
        this.listedOccurences = []
        this.messageCounts = {
            [MessageType.Horror]: 0,
            [MessageType.Hunch]: 0,
            [MessageType.Hint]: 0,
        }
        this.messageTypeFilter = null
        this.messageBadges = []
    }
}
