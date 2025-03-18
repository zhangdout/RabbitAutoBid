// Copied from Auth.js docs: https://authjs.dev/getting-started/session-management/protecting

export { auth as middleware } from "@/auth"

/*
保护路由（Protected Routes） 指的是 只有经过身份验证的用户 才能访问的页面或 API。
如果未登录的用户尝试访问这些页面，他们会被 重定向到登录页面，或者收到 "Unauthorized"（未授权） 的错误提示。
*/

export const config = {
    matcher: [
        '/session'
    ],
    pages: {
        signIn: '/api/auth/signin'
    }
}