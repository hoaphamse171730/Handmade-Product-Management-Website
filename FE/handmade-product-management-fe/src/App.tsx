import { GoogleOAuthProvider } from "@react-oauth/google";
import { useGoogleLogin } from "@react-oauth/google";

const CLIENT_ID =
  "463139815600-48lu701nfkms0et2o5p71pi12aqtnqre.apps.googleusercontent.com";

function App() {
  const login = useGoogleLogin({
    onSuccess: (tokenResponse) => {
      console.log("Token Response:", tokenResponse);
      localStorage.setItem("googleToken", tokenResponse.access_token);
      console.log("Token stored in localStorage:", tokenResponse.access_token);
    },
    onError: (error) => {
      console.error("Login Failed:", error);
    },
  });

  return (
    <div>
      <button
        onClick={() => login()}
        style={{ padding: "10px 20px", fontSize: "16px" }}
      >
        Sign in with Google 🚀
      </button>
    </div>
  );
}

const RootApp = () => (
  <GoogleOAuthProvider clientId={CLIENT_ID}>
    <App />
  </GoogleOAuthProvider>
);

export default RootApp;
