import { css, customElement, FASTElement, html, ref } from "@microsoft/fast-element"

import UsagiHappy from "./../../../assets/images/usagihappy.gif"

const styles = css`
    ::part(container) {
        border-width: 1px;
        border-style: dashed none none none;
    }

    ::part(content) {
        font-family: monospace;
    }

    input {
        font-family: monospace;
    }
`

const template = html`
  <sun-container 
  draggable 
  emoji="💖"
  color="#ff7272" 
  top="55%" 
  left="75%">
    <style>
      img {
        height: 20vh;
      }
    </style>
    <img class="usagi" ${ref("usagi")}></div>
  </sun-container>
`

export class Usagi extends FASTElement {
    usagi: HTMLImageElement

    connectedCallback(): void {
        super.connectedCallback() // ! don't forget this

        // console.log(this.usagi)
        this.usagi.src = UsagiHappy
    }
}

Usagi.define({
    name: "usagi-happy",
    template,
    styles,
    shadowOptions: { mode: "open", delegatesFocus: true },
})