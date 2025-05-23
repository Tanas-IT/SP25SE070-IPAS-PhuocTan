import { TableColumn } from "@/types";
import { GetEmployee } from "@/payloads";
import { TableCell, Tooltip, UserAvatar } from "@/components";
import { formatDate } from "@/utils";
import { Flex, Tag } from "antd";

export const EmployeeColumns: TableColumn<GetEmployee>[] = [
  {
    header: "Code",
    field: "user.userCode",
    accessor: (item) => <TableCell value={item.user.userCode} isCopyable={true} />,
    width: 200,
  },
  {
    header: "FullName",
    field: "user.fullName",
    accessor: ({ user }) => (
      <Flex justify="flex-start" align="center" gap={12}>
        <UserAvatar avatarURL={user?.avatarURL} />
        <Flex justify="center" style={{ width: "100%" }}>
          <TableCell value={user.fullName} />
        </Flex>
      </Flex>
    ),
    width: 250,
  },
  {
    header: "Email",
    field: "user.email",
    accessor: ({ user }) => <TableCell value={user.email} />,
    width: 250,
  },
  {
    header: "Role",
    field: "roleName",
    accessor: (item) => <TableCell value={item.roleName} />,
    width: 180,
  },
  {
    header: "Skills",
    field: "skills",
    accessor: (item) => {
      return (
        <Flex gap={4} wrap="wrap" justify="center">
          {item.skills?.length > 0 && item.skills[0].skillName ? (
            item.skills.map((skill, index) => (
              <Tooltip key={index} title={`Score: ${skill.scoreOfSkill}`}>
                <Tag color="green" style={{ margin: 0, cursor: "pointer" }}>
                  {skill.skillName}: {skill.scoreOfSkill}
                </Tag>
              </Tooltip>
            ))
          ) : (
            <Tag color="default">No skills</Tag>
          )}
        </Flex>
      );
    },
    width: 250,
  },
  {
    header: "Gender",
    field: "user.gender",
    accessor: (item) => <TableCell value={item.user.gender} />,
    width: 150,
  },
  {
    header: "Status",
    field: "isActive",
    accessor: (item) => (
      <Tag color={item.isActive ? "green" : "red"}>{item.isActive ? "Active" : "Inactive"}</Tag>
    ),
    width: 150,
  },
  {
    header: "Phone Number",
    field: "user.phoneNumber",
    accessor: (item) => <TableCell value={item.user.phoneNumber} />,
    width: 150,
  },
  {
    header: "DOB",
    field: "user.dob",
    accessor: (item) => <TableCell value={formatDate(item.user.dob)} />,
    width: 150,
  },
  {
    header: "Create Date",
    field: "user.createDate",
    accessor: (item) => <TableCell value={formatDate(item.user.createDate)} />,
    width: 150,
  },
];
