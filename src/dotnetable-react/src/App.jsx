import { BrowserRouter, Route, Routes } from 'react-router-dom';
import Layout from './components/Layout.jsx';
import Home from './pages/Home.jsx';
import { BlogList, BlogPost, NotFound } from './pages/Blog.jsx';
import CmsPage from './pages/CmsPage.jsx';
import FormPage from './pages/FormPage.jsx';
import Contact from './pages/Contact.jsx';
import { ShopList, ProductDetail } from './pages/Shop.jsx';

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<Layout />}>
          <Route index element={<Home />} />
          <Route path="blog" element={<BlogList />} />
          <Route path="blog/:slug" element={<BlogPost />} />
          <Route path="shop" element={<ShopList />} />
          <Route path="shop/:slug" element={<ProductDetail />} />
          <Route path="page/:slug" element={<CmsPage />} />
          <Route path="form/:slug" element={<FormPage />} />
          <Route path="contact" element={<Contact />} />
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </BrowserRouter>
  );
}
