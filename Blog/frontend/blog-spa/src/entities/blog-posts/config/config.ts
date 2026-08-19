import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

// Relative to the axios baseURL, and must equal the server's [Route(...)] exactly — repeating the base here
// sends requests to /api/api/... See entities.setup → The URL contract.
const api = "/blog-posts"

const config: IConfig = {
    id: Entity.name,
    key: "BlogPost",
    isComplex: true, // has relations, an owned collection and a long content field -> details page

    routePrefix: "blog-posts",
    baseQueryParams: {}, // Tags is Details-only (loaded eagerly there); Category is unconditional on every row
    initialQuery: {},

    overviewTitle: "blogPosts",
    detailsTitle: "blogPost",
    description: "blogPost.description",
    icon: "bi bi-file-earmark-text",

    defaultPageSize: 15,

    api, // every *Url below defaults to `api` when omitted; keep only the ones you override
    searchUrl: api + "/search", // counted search endpoint — the overview pages through it (every controller exposes /search)
    saveUrl: api, // resource base — update/remove append /{$id} themselves
}

export default config
