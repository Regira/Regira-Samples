import type { IConfig } from "@regira/modules/vue/entities"
import Entity from "../data/Entity"

const api = "/credit-policies"

const config: IConfig = {
    id: Entity.name,
    key: "CreditPolicy",
    isComplex: false, // a handful of numeric fields, no relations - edit in a modal

    routePrefix: "credit-policies",
    baseQueryParams: {},
    initialQuery: {},

    overviewTitle: "creditPolicies",
    detailsTitle: "creditPolicy",
    description: "creditPolicy.description",
    icon: "bi bi-sliders",

    defaultPageSize: 10,

    api,
    searchUrl: api + "/search",
    saveUrl: api
}

export default config
