import { ref } from "vue"
import { defineStore } from "pinia"
import { useAxios } from "@regira/modules/vue/http"
import { onAuthenticated, useAuthStore } from "@regira/modules/vue/auth"
import type { Entity as Person } from "@/entities/persons"

export interface Me {
    userId?: string
    roles: Array<string>
    isStaff: boolean
    isAdmin: boolean
    person?: Person
}

/** GET /me — the signed-in account's roles and linked person profile (comment author, ticket owner). */
export const useMeStore = defineStore("me", () => {
    const me = ref<Me>()
    let loadedFor: string | undefined

    async function load() {
        const token = useAuthStore().authData?.token
        if (token && token === loadedFor && me.value) return me.value
        const { data } = await useAxios().get("/me")
        me.value = data.item as Me
        loadedFor = token
        return me.value
    }
    function init() {
        onAuthenticated(() => load())
    }
    return { me, load, init }
})
