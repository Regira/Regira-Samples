import { parseCsv } from "@/infrastructure/csv"
import { useConfig } from "@/app-config"

// The picker's emoji list: public/data/emoji-picker.csv (Group;Emoji;Keywords). Loaded on first use, then cached.

export interface EmojiEntry {
    group: string
    emoji: string
    keywords: string
}

let catalogue: Promise<Array<EmojiEntry>> | undefined

export function loadEmojiCatalogue(): Promise<Array<EmojiEntry>> {
    catalogue ??= fetch(`${useConfig().baseUrl}/data/emoji-picker.csv`, { cache: "no-cache" }) // revalidate after a redeploy
        .then((response) => {
            if (!response.ok) throw new Error(`${response.status} ${response.statusText}`)
            return response.text()
        })
        .then((text) => {
            const [header, ...lines] = parseCsv(text)
            const col = (name: string) => header!.findIndex((h) => h.trim().toLowerCase() === name)
            const [group, emoji, keywords] = ["group", "emoji", "keywords"].map(col)
            return lines
                .map((l) => ({ group: l[group!] ?? "", emoji: (l[emoji!] ?? "").trim(), keywords: l[keywords!] ?? "" }))
                .filter((e) => e.emoji)
        })
        .catch((ex) => {
            catalogue = undefined // try again next time
            throw ex
        })
    return catalogue
}
