import React, { ReactNode, useEffect, useState } from "react";

import style from "./ManagementLayout.module.scss";
import { Footer, HeaderAdmin, Loading, SidebarAdmin } from "@/components";
import { Breadcrumb, Flex, Layout } from "antd";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { getRoleId, isValidBreadcrumb } from "@/utils";
import {
  useFarmExpiration,
  useRequireAuth,
  useToastFromLocalStorage,
  useToastMessage,
} from "@/hooks";
import { LOCAL_STORAGE_KEYS, MESSAGES, UserRole, UserRolesStr } from "@/constants";
import { PATHS } from "@/routes";
const { Header, Content } = Layout;

interface ManagementLayoutProps {
  children: ReactNode;
  hideSidebar?: boolean;
}

const ManagementLayout: React.FC<ManagementLayoutProps> = ({ children, hideSidebar }) => {
  useToastMessage();
  useToastFromLocalStorage();
  // useFarmExpiration();
  const navigate = useNavigate();
  const location = useLocation();
  const pathnames = location.pathname.split("/").filter((x) => x);
  const isTokenChecked = useRequireAuth();
  const [isUser, setIsUser] = useState<boolean>(false);

  useEffect(() => {
    const roleId = getRoleId();
    if (roleId === UserRolesStr.User) {
      setIsUser(true);
      localStorage.setItem(LOCAL_STORAGE_KEYS.ERROR_MESSAGE, MESSAGES.NO_PERMISSION);
      navigate(PATHS.FARM_PICKER);
    }
  }, [navigate]);

  if (!isTokenChecked || isUser) return <Loading />;

  const breadcrumbItems = pathnames
    .filter((path) => !isValidBreadcrumb(path))
    .map((path, index, filteredPaths) => {
      const pathTo = `/${pathnames.slice(0, index + 1).join("/")}`;
      const title = path.charAt(0).toUpperCase() + path.slice(1).toLowerCase();

      const isLastItem = index === filteredPaths.length - 1;

      return isLastItem
        ? { title: <span className={style.active}>{title}</span> }
        : {
            title: (
              <Link className={style.noneEvent} to={pathTo}>
                {title}
              </Link>
            ),
          };
    });

  return (
    <Flex className={style.mainContainer}>
      {!hideSidebar && <SidebarAdmin />}
      <Layout className={style.layout}>
        <HeaderAdmin />
        <Content className={style.contentWrapper}>
          <Flex className={style.content}>
            <Breadcrumb items={breadcrumbItems} />
            {children}
          </Flex>
        </Content>
        <div className={style.footerWrapper}>
          <Footer isManagement={true} />
        </div>
      </Layout>
    </Flex>
  );
};

export default ManagementLayout;
