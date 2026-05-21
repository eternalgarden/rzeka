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

// Designating type with ': ViewTemplate<OccurencesContainerElement>' is not necessary
// it's a 'typescript' thing, here for clarity,
// its probably a good practice but honestly it does a bit occlude the code
const LITTLE_TEMPLATE: ViewTemplate<GefildeDesVorkommen> = html<GefildeDesVorkommen>`
    <div class="menu">
        <div class="header">😼🗝️🗃️ occurences</div>
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

    // 🦄 Purposefully public
    occurenceArchive: OccurencesArchive

    // 🎯 Currently selected matter guid — drives the causality tree window.
    // Null means nothing is selected.
    readonly selectedMatterGuid$ = new BehaviorSubject<string | null>(null)

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

    private applyFilter() {
        this.listedOccurences = this.allOccurences.filter(occ =>
            this.matchesSearch(occ)
        )
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
                this.allOccurences.unshift(occ)
                if (this.matchesSearch(occ))
                    this.listedOccurences = [occ, ...this.listedOccurences]
            })

        this.lifecycleSubscriptions.push(sub)
    }

    setFocusToSearchbar() {
        this.searchbar.focus()
    }

    clearOccurences() {
        this.allOccurences = []
        this.listedOccurences = []
    }
}
