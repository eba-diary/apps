export { }

declare global {
    interface JQuery {
        /** Manually defining Material Select from MDB into the jQuery namespace  */
        materialSelect(): JQuery;
    }
}