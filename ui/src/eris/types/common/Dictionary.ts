export class Dictionary<T> {
    constructor() {
        this.data = {}
    }

    data: Record<string, T>
}
