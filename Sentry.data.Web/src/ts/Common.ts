import * as toastr from 'toastr';

export namespace Common {
    export async function copyTextToClipboard(text: string): Promise<void> {
        await navigator.clipboard.writeText(text);
        toastr.success("Copied " + text + " to clipboard");
    }
}