import {
    buttonStyles,
    fastAccordionItem,
    fastButton,
    fastCard,
    provideFASTDesignSystem,
} from "@microsoft/fast-components"
import * as fastElement from "@microsoft/fast-element"
import { FloatyBox } from "../components/common/FloatyBox"
import { ImageLoader } from "../components/other/ImageLoader"
import { MatterItem } from "../components/occurences/MatterItem"
import { MessageItem } from "../components/occurences/MessageItem"
import { GefildeDesVorkommen } from "../components/GefildeDesVorkommen"
import { Usagi } from "../components/other/Usagi"
import { WHDIGLoader } from "../components/other/WHDIGLoader"

// export const initiateErisDesignSystem = () =>
//     provideFASTDesignSystem()
//         .withPrefix("sanctuary") // TODO 🦧 why those components work without me using this prefix?
//         .register(
//             fastCard(),
//             fastAccordionItem({
//                 baseName: "accordion-item",
//             }),
//             fastButton({
//                 styles: (ctx, def) => fastElement.css`
//             ${buttonStyles(ctx, def)}
//                 /* add your style augmentations here */
//             `,
//             }),
//             FloatyBox,
//             GefildeDesVorkommen,
//             // SpellItem,
//             MatterItem,
//             MessageItem,
//             Usagi,
//             WHDIGLoader,
//             ImageLoader
//         )
