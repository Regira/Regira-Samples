import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

// Relative to the axios baseURL, and must equal the server's [Route(...)] exactly — repeating the base here
// sends requests to /api/api/... See entities.setup → The URL contract.
const api = "/articles"

const config: IConfig = {
    id: Entity.name,
    key: "Article",
    isComplex: true,

    routePrefix: "articles",
    // the API gates both behind its named [Flags] ArticleIncludes; list rows show the list name + category chips
    baseQueryParams: { includes: ["Categories", "ShoppingList"] },
    initialQuery: {}, // route query for the GENERATED nav link ONLY — lost on refresh/deep-link. A default sortBy or includes belongs in baseQueryParams

    overviewTitle: "articles",
    detailsTitle: "article",
    description: "article.description",
    icon: "bi bi-bag-check",

    defaultPageSize: 25,

    api, // every *Url below defaults to `api` when omitted; keep only the ones you override
    searchUrl: api + "/search", // counted search endpoint — the overview pages through it (every controller exposes /search)
    saveUrl: api, // resource base — update/remove append /{$id} themselves
}

export default config
