// import {
//     FASTElement,
//     css,
//     customElement,
//     html,
//     observable,
//     when,
// } from "@microsoft/fast-element"
// import { ISpellOccurence } from "../../types/spell-occurence-types"
// import {
//     SpellOccurenceCategory,
//     SpellSchool,
// } from "../../types/rzeka-enum-types"
// import { getFewGuidCharacters } from "./common-occurence"
// import * as moment from "moment"
// import { ISerlializableBindingSpell } from "../../types/rzeka-base-types"
// import { OccurencesContainerElement } from "./OccurencesContainerElement"
// import { Subscription } from "rxjs"

// const styles = css`
//     .occurence-item {
//         border-bottom: calc(var(--stroke-width) * 1px) solid #000000;
//         background-color: #6900ff;
//     }

//     ::part(heading):hover {
//         background-color: rgb(255 116 254);
//     }

//     .region-content {
//         display: flex;
//         flex-direction: column;
//         row-gap: 2px;
//         align-items: left;
//     }

//     .emojiguid {
//         display: flex;
//         flex-direction: column;
//         align-items: center;
//     }

//     .details {
//         align-self: flex-end;
//         font-size: 0.9em;
//     }

//     ul {
//         list-style-type: hiragana;
//         margin: 0.2em 0;
//         color: #ffe300;
//     }

//     .red {
//         color: red;
//     }

//     .green {
//         color: #00ff00;
//     }

//     .filterMatch {
//         background-color: red;
//     }
// `

// const spellWhoTemplate = html<SpellItem>`
//     <span>${x => `🐈 ${x.spellOccurence.spellOccurenceCategory}`}</span>
//     <div class="who">
//         <span>Who</span>
//         <ul>
//             <li>Type: ${x => x.spellOccurence.spell.Who.WhosType.name}</li>
//             <li>
//                 Namespace: ${x => x.spellOccurence.spell.Who.WhosType.namespace}
//             </li>
//             <li>
//                 Instance:
//                 ${x =>
//                     x.spellOccurence.spell.Who.WhosDescription != undefined
//                         ? x.spellOccurence.spell.Who.WhosDescription
//                         : "unnamed"}
//             </li>
//         </ul>
//     </div>
// `

// const spellIngredientsTemplate = html<SpellItem>`
//     ${when(
//         x => x.spellOccurence.spell.spellSchool !== SpellSchool.Stranding,
//         html<SpellItem>`
//             <div class="ingredients">
//                 <span>Ingredients</span>
//                 <ul
//                     :innerHTML=${x =>
//                         getSpellIngredients(x.spellOccurence.spell)}></ul>
//             </div>
//         `
//     )}
// `

// const spellDetailsTemplate = html<SpellItem>`
//     <span class="details">${x => x.spellOccurence.spell.guid}</span>
//     <span class="details">
//         ${x =>
//             moment
//                 .unix(x.spellOccurence.timestamp)
//                 .format("MMMM Do YYYY, h:mm:ss a")}
//     </span>
// `

// /* SPELL TEMPLATE */
// const template = html<SpellItem>`
//     <sanctuary-accordion-item
//         class="occurence-item ${x => x.filterClass}"
//         tabindex="-1">
//         <div
//             slot="start"
//             class="emojiguid">
//             <span>${x => selectItemEmoji(x.spellOccurence)}</span>
//             <span>
//                 ${x => getFewGuidCharacters(x.spellOccurence.spell.guid)}
//             </span>
//         </div>
//         <span slot="heading">${x => `${x.spellOccurence.spell.title}'s`}</span>
//         <span slot="collapsed-icon">🐒</span>
//         <span slot="expanded-icon">🧠</span>
//         <div class="region-content">
//             ${spellWhoTemplate} ${spellIngredientsTemplate}
//             ${spellDetailsTemplate}
//         </div>
//     </sanctuary-accordion-item>
// `

// @customElement({
//     name: "spell-item",
//     template,
//     styles,
// })
// export class SpellItem extends FASTElement {
//     @observable filterClass: string = ""
//     @observable container: OccurencesContainerElement
//     @observable spellOccurence: ISpellOccurence

//     filterSubscription: Subscription

//     containerChanged(
//         _: OccurencesContainerElement,
//         container: OccurencesContainerElement
//     ) {
//         if (this.filterSubscription != undefined)
//             this.filterSubscription.unsubscribe()

//         this.filterSubscription = container.filterObservable.subscribe(val => {
//             if (val.length < 3) this.filterClass = ""
//             else {
//                 if (this.spellOccurence.containsText(val))
//                     this.filterClass = "filterMatch"
//                 else this.filterClass = "filterFail"
//             }
//         })
//     }
// }

// function selectItemEmoji(occ: ISpellOccurence): string {
//     let emoji = "" // '📜'

//     const spellSchool: SpellSchool = occ.spell.spellSchool
//     if (spellSchool == SpellSchool.Stranding) {
//         emoji += "🎫"
//     } else if (spellSchool == SpellSchool.Looming) {
//         emoji += "🏓"
//     } else if (spellSchool == SpellSchool.Weaving) {
//         emoji += "♻️"
//     }

//     if (occ.spellOccurenceCategory == SpellOccurenceCategory.Created)
//         emoji += "📜"
//     else if (occ.spellOccurenceCategory == SpellOccurenceCategory.NoMana)
//         emoji += "🌋"
//     else if (occ.spellOccurenceCategory == SpellOccurenceCategory.HasMana)
//         emoji += "🌌"
//     else if (occ.spellOccurenceCategory == SpellOccurenceCategory.Forgotten)
//         emoji += "🕷️"
//     else emoji += "⚠️"

//     return emoji
// }

// function getSpellIngredients(spell: any): string {
//     let returnedListMarkup: string = ""

//     const bindingSpell: ISerlializableBindingSpell =
//         spell as ISerlializableBindingSpell

//     const keys = Object.keys(bindingSpell.ingredients.data)

//     for (let i = 0; i < keys.length; i++) {
//         const key: string = keys[i]
//         const el = bindingSpell.ingredients.data[key]

//         returnedListMarkup =
//             returnedListMarkup +
//             `<li class=${el ? "green" : "red"}>${key} : ${el}</li>`
//     }

//     return returnedListMarkup
// }
