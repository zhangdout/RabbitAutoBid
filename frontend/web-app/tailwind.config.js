/** @type {import('tailwindcss').Config} */
import flowbite from 'flowbite-react/tailwind';
module.exports = {
  content: [
    "./pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
    flowbite.content(),
  ],
  theme: {
    extend: {},
  },
  plugins: [
    flowbite.plugin()
  ],
};
