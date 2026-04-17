/** @type {import('tailwindcss').Config} */
export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      colors: {
        midnight: '#020a19',
        navy: '#0b2b63',
        blue: '#1e6de3',
        orange: '#ff8a1f',
        black: '#05070d',
      },
      fontFamily: {
        heading: ['"Chakra Petch"', 'sans-serif'],
        body: ['"Space Grotesk"', 'sans-serif'],
        mono: ['"IBM Plex Mono"', 'monospace'],
      },
      keyframes: {
        rise: {
          '0%': { opacity: '0', transform: 'translateY(18px) scale(0.98)' },
          '100%': { opacity: '1', transform: 'translateY(0) scale(1)' },
        },
        signal: {
          '0%, 100%': { boxShadow: '0 0 0 0 rgba(30, 109, 227, 0.8)' },
          '70%': { boxShadow: '0 0 0 10px rgba(30, 109, 227, 0)' },
        },
      },
      animation: {
        rise: 'rise 650ms cubic-bezier(0.16, 1, 0.3, 1) both',
        signal: 'signal 2.5s infinite',
      },
      boxShadow: {
        panel: '0 20px 35px -25px rgba(0, 0, 0, 0.7)',
      },
    },
  },
  plugins: [],
}

