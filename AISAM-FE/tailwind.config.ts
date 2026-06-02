import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/app/**/*.{ts,tsx}",
    "./src/components/**/*.{ts,tsx}",
    "./src/features/**/*.{ts,tsx}",
    "./src/lib/**/*.{ts,tsx}"
  ],
  theme: {
    extend: {
      colors: {
        background: "#f7f4eb",
        foreground: "#1c1917",
        card: "#fffdf7",
        border: "#d6d0be",
        primary: {
          DEFAULT: "#1e4d3a",
          foreground: "#f8f6ee"
        },
        secondary: {
          DEFAULT: "#d9b26f",
          foreground: "#2f2616"
        },
        muted: {
          DEFAULT: "#ece6d7",
          foreground: "#5f5a4c"
        },
        destructive: {
          DEFAULT: "#a23a32",
          foreground: "#fff8f7"
        }
      },
      borderRadius: {
        xl: "1rem"
      },
      boxShadow: {
        panel: "0 16px 40px rgba(28, 25, 23, 0.08)"
      }
    }
  },
  plugins: []
};

export default config;
