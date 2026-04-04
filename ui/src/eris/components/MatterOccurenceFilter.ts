import { MatterOccurenceCategory } from "../types/occurence-categories/MatterOccurenceCategory"

export class MatterOccurenceFilter {
    private filterString: string = ""
    private displayedMatterOccurences: Map<MatterOccurenceCategory, boolean>

    displayMatterOccurences: boolean = true
    displaySpellOccurences: boolean = false

    constructor() {
        this.displayedMatterOccurences = new Map<
            MatterOccurenceCategory,
            boolean
        >()
            .set(MatterOccurenceCategory.Shaped, true)
            .set(MatterOccurenceCategory.Received, false)
            .set(MatterOccurenceCategory.Error, false)
    }

    isMatterCategoryDisplayed(category: MatterOccurenceCategory): boolean {
        const value = this.displayedMatterOccurences.get(category)
        if (value === undefined)
        {
            console.log(`🔥 Undefined filter matter category: ${category}`)
            return false
        }
        return value
    }

    setMatterOccurenceDisplay(
        category: MatterOccurenceCategory,
        value: boolean
    ) {
        this.displayedMatterOccurences.set(category, value)
    }

    setFilteredString(text: string) {
        this.filterString = text
    }

    hasFilterString(): boolean {
        return this.filterString.length > 2
    }

    getFilterString(): string {
        return this.filterString
    }
}
