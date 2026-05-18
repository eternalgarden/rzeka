export const ON_NEW_RAW_OCCURENCE = "onNewRawOccurence"
export const ON_SET_FOCUS_TO_SEARCHBAR ="onFocusToSearchbar"
export const ON_CLEAR_OCCURENCES ="onClearOccurences"
export const ON_CONNECTION_STATUS_CHANGE = "onConnectionStatusChange"

export type ConnectionStatus =
    | { status: "connected" }
    | { status: "disconnected"; reconnectAt: number | null }