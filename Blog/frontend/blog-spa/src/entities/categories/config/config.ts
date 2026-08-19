import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

// Relative to the axios baseURL, and must equal the server's [Route(...)] exactly — repeating the base here
// sends requests to /api/api/... See entities.setup → The URL contract.
const api = "/categories"

const config: IConfig = {
    id: Entity.name,
    key: "Category",
    isComplex: true, // Category has a description + computed post count, so it gets a details page

    routePrefix: "categories",
    baseQueryParams: {},
    initialQuery: {},

    overviewTitle: "categories",
    detailsTitle: "category",
    description: "category.description",
    icon: "bi bi-folder2",

    defaultPageSize: 10, // initial overview page size (raise it to show more per page, up to the server's MaxPageSize)

    api, // every *Url below defaults to `api` when omitted; keep only the ones you override
    searchUrl: api + "/search", // counted search endpoint — the overview pages through it (every controller exposes /search)
    saveUrl: api, // resource base — update/remove append /{$id} themselves
}

export default config
