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

    // ☔ Passed matter occurence elements
    @observable matterOccurenceCategory: MatterOccurenceCategory
    @observable timestamp: number
    @observable matterType: Type
    @observable matterData: MatterData
    @observable matterGuid: string
    @observable matterContent: any
    @observable isVisible: boolean
    @observable containsSearchedText: boolean

    /* 🐺🗃️🗝️ Two shady 'situational' fields */
    /* Clean solution would be to split MatterItem into two separate Elements */
    /* But it is still quite readable and there won't be more matter occurence types. */
    @observable spellGuid: string = "" // Only Shaped 
    @observable listOfReceivingSpells: string[] = [] // Only Received
    
    spellArchive: SpellArchive
    matterArchive: MatterArchive

    filterSubscription: Subscription
    receivedSubscription: Subscription | undefined

    wildcardSubscription: Subscription

    connectedCallback(): void {
        // 🌲🗃️ Archives Access
        this.spellArchive = this.gefilde.occurenceArchive.spellsArchive
        this.matterArchive = this.gefilde.occurenceArchive.matterArchive

        // 🌲🗝️ Archivial keys
        this.matterGuid = this.occurence.matterGuid
        // this.spellGuid = this.occurence.spellGuid

        // 🪶 Basic matter info
        this.matterOccurenceCategory = this.occurence.matterOccurenceCategory
        this.timestamp = this.occurence.timestamp
        this.matterType = this.matterArchive.getMatterType(this.matterGuid)
        this.matterData = this.matterArchive.getMatterData(this.matterGuid)

        this.matterContent = this.matterData.content

        // 🪞 Filter subscription
        this.filterSubscription = this.gefilde.newFilterObservable.subscribe(
            filter => {
                this.isVisible = filter.isMatterCategoryDisplayed(
                    this.matterOccurenceCategory
                )
                if (!this.isVisible) return

                this.containsSearchedText =
                    filter.hasFilterString() &&
                    containsText(filter.getFilterString(), this.matterType)
            }
        )

        if (this.matterOccurenceCategory === MatterOccurenceCategory.Received) {
            this.wildcardSubscription = this.getReceivalSubscription()
        }

        // console.log(this.matterType)

        super.connectedCallback()
    }

    disconnectedCallback(): void {
        this.filterSubscription?.unsubscribe()
        this.receivedSubscription?.unsubscribe()
        this.wildcardSubscription?.unsubscribe()

        super.disconnectedCallback()
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
                // console.log("🧶")
                this.listOfReceivingSpells = receivers
            }
        )
    }
}

function containsText(text: string, matterType: Type) {
    return matterType.name.toLowerCase().includes(text)
}
