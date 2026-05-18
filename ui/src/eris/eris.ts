
// What is dis
import { GefildeDesVorkommen } from "./components/GefildeDesVorkommen"

/* ⛰️ components */
import "../components/common/elements/foldout/Foldout.ts"
import "./components/common/FloatyBox.ts"
import "./components/other/ImageLoader.ts"
import "./components/occurences/MatterItem.ts"
import "./components/occurences/MessageItem.ts"
import "./components/GefildeDesVorkommen.ts"
import "./components/causality/CausalityTree.ts"
import "./components/other/Usagi.ts"
import "./components/other/WHDIGLoader.ts"

import { loadTestDataIfLocal } from "./helpers/loadTestDataIfLocal"
import { connectToDebugServer } from "./connection/debugServerConnection"

/* 🌋 events */
import {
    ON_CLEAR_OCCURENCES,
    ON_NEW_RAW_OCCURENCE,
    ON_SET_FOCUS_TO_SEARCHBAR,
} from "./types/common/ErisEvents"

/* 🎨 styles */
import "../components/common/styles/font.css"
import "../components/common/styles/fontsMediengestaltung.css"
import rzekaBackground from "../assets/images/rzeka.jpeg"

/* 📜 Components */
import "./components/other/WHDIGLoader"
import "./components/other/Usagi"

const gefildeDesVorkommen: GefildeDesVorkommen = document.getElementById(
    "eris-realm-events"
) as GefildeDesVorkommen

declare global {
    interface Window {
        OnNewOccurence: (json: string) => void
        SetFocusToSearchbar: () => void
        OnImageData: (data: string) => void
        ClearOccurences: () => void
    }
}

document.body.style.backgroundImage = `url(${rzekaBackground})`

/*
HOW IT GOES?

Pre-info:

The separation beween 'Occurences' and 'Archived Occurences' is
necessary due to the original being a basically raw data-form copy from
what is coming here from Unity/C#, Archived version is one processed by
eris to be then rendered and used.

This separation is useful because you can safely manipulate Archived version
code, while the 'Raw' ones *must* be coordinated carefully and manually
with changes in C# code, derp. 🧶

The Process:

First we receive the most general notification form: An occurence.

An occurence can be of one of three categories:
- Spell Occurence (7 sub-categories like 'Created', 'Forgotten', 'NoMana')
- Matter Occurence (2 sub-categories: 'Shaped' or 'Received')
- Message Occurence (a plain log)

An occurence is thus a general timep-stamped event that contains further
data about what happened.

Spell Occurences define Spells.

Matter Occurences define Matter.
- Shaped Matter Occurences contain new Matter data.
- Received Matter Occurences only contain a guid of referenced Matter.
*/

// Legacy: window.OnNewOccurence still works for WHDIG file loading and test data
window.OnNewOccurence = (json: string) => {
    const rawEvent = new CustomEvent(ON_NEW_RAW_OCCURENCE, { detail: json })
    document.dispatchEvent(rawEvent)
}

window.SetFocusToSearchbar = () => {
    const rawEvent = new CustomEvent(ON_SET_FOCUS_TO_SEARCHBAR)
    document.dispatchEvent(rawEvent)
}

// 🛠️ TOdO this is not functioning anyway, its an empty function in Gefilde
window.ClearOccurences = () => {
    gefildeDesVorkommen.clearOccurences()
    const rawEvent = new CustomEvent(ON_CLEAR_OCCURENCES)
    document.dispatchEvent(rawEvent)
}

// <usagi-happy> is decorative and opt-in via the #usagi URL hash.
if (!window.location.hash.includes("usagi")) {
    document.querySelector("usagi-happy")?.remove()
}

// Connect to the live debug server (WebSocket)
// Falls back gracefully if the game isn't running - reconnects automatically
connectToDebugServer()

// Still supports #local for loading test data from bundled JSON
loadTestDataIfLocal()
// initiateErisDesignSystem()
