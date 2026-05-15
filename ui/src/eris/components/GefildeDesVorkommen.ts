import {
    FASTElement,
    ViewTemplate,
    customElement,
    html,
    css,
    observable,
    repeat,
    ref,
} from "@microsoft/fast-element"

import { MatterOccurenceCategory } from "../types/occurence-categories/MatterOccurenceCategory"
import { OccurenceCategory } from "../types/occurence-categories/OccurenceCategory"
import {
    BehaviorSubject,
    Observable,
    Subscription,
    combineLatest,
    fromEvent,
} from "rxjs"
import { filter, map, scan, share } from "rxjs/operators"
import { IArchivedOccurence } from "../types/occurences-archived/IArchivedOccurence"
import { OccurencesArchive } from "../types/archives/OccurencesArchive"
import { MatterOccurenceFilter } from "./MatterOccurenceFilter"
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

    .menu {
        position: sticky;
        top: 0;
        z-index: 1;
        width: 100%;
        display: flex;
        flex-direction: column;
        background-color: #17131385;
        color: #00ffc6;
        backdrop-filter: sepia(90%);
        padding: 0.3rem;
        font-family: monospace;
    }

    div.options {
        display: flex;
    }

    input.searchbar {
        height: 22px;
        background-color: transparent;
        color: #ffef00;
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
    }

    option {
        background-color: transparent;
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
        <div class="options">
            <span>😼🗝️🗃️ </span>
            <input
                ${ref("shapedMatterCheckbox")}
                type="checkbox"
                name="shapedMatter"
                checked />
            <label for="shapedMatter">shaped</label>
            <input
                ${ref("receivedMatterCheckbox")}
                type="checkbox"
                name="receivedMatter"
                checked />
            <label for="receivedMatter">received</label>
        </div>
        <input
            class="searchbar"
            ${ref("searchbar")}
            type="text"
            tabindex="0" />
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
    shapedMatterCheckbox: HTMLInputElement
    receivedMatterCheckbox: HTMLInputElement
    searchbar: HTMLInputElement

    // 🕊️ Displayed occurences
    @observable listedOccurences: IArchivedOccurence[] = []

    // 💎 Filtering
    private filter: MatterOccurenceFilter
    private shapedMatterFilter: Observable<boolean>
    private receivedMatterFilter: Observable<boolean>
    private searchbarFilter: Observable<string>

    // 🦄 Purposefully public
    occurenceArchive: OccurencesArchive
    newFilterObservable: Observable<MatterOccurenceFilter>

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
        this.shapedMatterFilter = fromEvent(
            this.shapedMatterCheckbox,
            "click"
        ).pipe(
            map(_ => {
                // console.log(this.shapedMatterCheckbox.checked)
                return this.shapedMatterCheckbox.checked
            }),
            share({
                connector: () =>
                    new BehaviorSubject(this.shapedMatterCheckbox.checked),
            })
        )

        this.receivedMatterFilter = fromEvent(
            this.receivedMatterCheckbox,
            "click"
        ).pipe(
            map(_ => {
                // console.log(this.receivedMatterCheckbox.checked)
                return this.receivedMatterCheckbox.checked
            }),
            share({
                connector: () =>
                    new BehaviorSubject(this.receivedMatterCheckbox.checked),
            })
        )

        this.searchbarFilter = fromEvent(this.searchbar, "input").pipe(
            map(x => {
                const value = (x.target as HTMLInputElement).value
                console.log(value)
                return value
            }),
            share({
                connector: () => new BehaviorSubject(""),
            })
        )

        const accumulatorSeed = new MatterOccurenceFilter()

        this.newFilterObservable = combineLatest([
            this.shapedMatterFilter,
            this.receivedMatterFilter,
            this.searchbarFilter,
        ]).pipe(
            scan((acc, next) => {
                const [isDisplayShaped, isDisplayReceived, searchFilter] = next

                acc.setMatterOccurenceDisplay(
                    MatterOccurenceCategory.Shaped,
                    isDisplayShaped
                )
                acc.setMatterOccurenceDisplay(
                    MatterOccurenceCategory.Received,
                    isDisplayReceived
                )
                acc.setFilteredString(searchFilter)
                return acc
            }, accumulatorSeed),
            share({
                connector: () => new BehaviorSubject(accumulatorSeed),
                resetOnError: false,
                resetOnComplete: false,
                resetOnRefCountZero: false,
            })
        )
    }

    private registerOccurenceArchiveListeners() {
        const sub: Subscription = this.occurenceArchive.newProcessedOccurence
            .pipe(
                // TODO currently only handling matter and message occurences
                filter(
                    x =>
                        x.occurenceCategory === OccurenceCategory.Matter ||
                        x.occurenceCategory === OccurenceCategory.Message
                )
            )
            .subscribe(occ => {
                const count = this.listedOccurences.unshift(occ)
            })

        this.lifecycleSubscriptions.push(sub)
    }

    setFocusToSearchbar() {
        this.searchbar.focus()
    }

    clearOccurences() {
        this.listedOccurences = []
    }
}
