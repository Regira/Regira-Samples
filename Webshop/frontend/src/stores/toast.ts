import { defineStore } from "pinia"

export interface ToastMessage {
  id: number
  text: string
  variant: "success" | "error" | "info"
}

let nextId = 1

export const useToastStore = defineStore("toast", {
  state: () => ({
    messages: [] as ToastMessage[],
  }),
  actions: {
    push(text: string, variant: ToastMessage["variant"] = "info", timeout = 3200) {
      const id = nextId++
      this.messages.push({ id, text, variant })
      setTimeout(() => this.dismiss(id), timeout)
    },
    success(text: string) {
      this.push(text, "success")
    },
    error(text: string) {
      this.push(text, "error", 4500)
    },
    dismiss(id: number) {
      this.messages = this.messages.filter((m) => m.id !== id)
    },
  },
})
