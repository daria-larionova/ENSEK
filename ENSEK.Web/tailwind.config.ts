import type { Config } from "tailwindcss";

export default {
  content: [
    "./pages/**/*.{js,ts,jsx,tsx,mdx}",
    "./components/**/*.{js,ts,jsx,tsx,mdx}",
    "./app/**/*.{js,ts,jsx,tsx,mdx}",
  ],
  theme: {
    extend: {
      colors: {
        'apple-blue': '#0071e3',
        'apple-gray': {
          50: '#f5f5f7',
          100: '#e8e8ed',
          200: '#d2d2d7',
          300: '#b0b0b8',
          400: '#86868b',
          500: '#6e6e73',
          600: '#515154',
          700: '#424245',
          800: '#1d1d1f',
          900: '#000000',
        }
      },
    },
  },
  plugins: [],
} satisfies Config;
