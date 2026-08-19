import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

// Relative to the axios baseURL, and must equal the server's [Route(...)] exactly — repeating the base here
// sends requests to /api/api/... See entities.setup → The URL contract.
const api = "/categories"

const config: IConfig = {
    id: Entity.name,
    key: "Category",
    isComplex: true,

    routePrefix: "categories",
    baseQueryParams: {}, // Category is a "simple" entity server-side — ParentEntities/ChildEntities are eager-loaded unconditionally, no ?includes= flag exists to request
    initialQuery: {},

    overviewTitle: "categories",
    detailsTitle: "category",
    description: "category.description",
    icon: "bi bi-tags",

    defaultPageSize: 60,

    api, // every *Url below defaults to `api` when omitted; keep only the ones you override
    searchUrl: api + "/search", // counted search endpoint — the overview pages through it (every controller exposes /search)
    saveUrl: api, // resource base — update/remove append /{$id} themselves
}

export default config
