/** @type {import('tailwindcss').Config} */
module.exports = {
    content: [
        './Pages/**/*.cshtml', // Razor Pages Views
        './Views/**/*.cshtml', // MVC Views
        './wwwroot/**/*.html',
    ],
  theme: {
    extend: {},
  },
  plugins: [],
}

