import { Navigate, Route, Routes } from "react-router-dom";
import ProxiesPage from "./pages/Proxies";

export default function App() {
  return (
    <Routes>
      <Route index element={<Navigate to="/proxies" />} />
      <Route path="/proxies" element={<ProxiesPage />} />
    </Routes>
  );
}
